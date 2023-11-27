using SA3D.SA2Event.Animation;
using SA3D.SA2Event.Model;
using SA3D.Rendering;
using SA3D.Modeling.ObjectData;
using System;
using System.Numerics;
using SA3D.SA2CutscenePlayer.Effects;
using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Animation;

namespace SA3D.SA2CutscenePlayer.Player
{
    public partial class EventPlayer
    {
        public bool AnimateCamera { get; set; }

        public TailsTailAnimation[]? TailsAnimations { get; }

        private void UpdateAnimations(RenderContext context)
        {
            float motionTimestamp = (float)(Timestamp - CurrentSceneFrameOffset);

            foreach(EventEntry entity in CurrentScene.Entries)
            {
                entity.ProcessEntityAnimation(context, motionTimestamp);
            }

            for(int i = 0; i < Cutscene.ModelData.OverlayUpgrades.Length; i++)
            {
                int lut = OverlayUpgrade.UpgradeEventLUT[i];
                if(lut == -2 || _upgrades[lut])
                {
                    OverlayUpgrade upgrade = Cutscene.ModelData.OverlayUpgrades[i];
                    UpdateUpgradeTransforms(upgrade);
                }
            }

            UpdateTailsTails(context);
            UpdateUVAnimations();
            UpdateCamera(context, motionTimestamp);
        }

        private void UpdateUpgradeTransforms(OverlayUpgrade upgrade)
        {
            static void UpdateSingle(Node? model, Node? attach)
            {
                if(model == null || attach == null)
                {
                    return;
                }

                Matrix4x4 matrix = attach.GetWorldMatrix();
                model.Position = matrix.Translation;
                model.QuaternionRotation = Quaternion.CreateFromRotationMatrix(matrix);
            }

            UpdateSingle(upgrade.Model1, upgrade.Target1);
            UpdateSingle(upgrade.Model2, upgrade.Target2);
        }

        private void UpdateUVAnimations()
        {
            if(Cutscene.ModelData.SurfaceAnimations == null
                || _previousTimeStamp % 1.0f < Timestamp % 1.0f)
            {
                return;
            }

            foreach(TextureAnimSequence item in Cutscene.ModelData.SurfaceAnimations.TextureSequences)
            {
                int texID = item.TextureID;
                ushort newTexID = (ushort)(item.TextureID + ((int)Timestamp % item.TextureCount));

                foreach(SurfaceAnimationBlock uvAnimBlock in Cutscene.ModelData.SurfaceAnimations.AnimationBlocks)
                {
                    if(uvAnimBlock.Model.Attach is not ChunkAttach atc || atc.PolyChunks == null)
                    {
                        throw new InvalidOperationException();
                    }

                    foreach(SurfaceAnimation? anim in uvAnimBlock.Animations)
                    {
                        if(anim == null)
                        {
                            continue;
                        }

                        if(anim.TextureID == texID)
                        {
                            int meshIndex = 0;
                            foreach(PolyChunk? polyChunk in atc.PolyChunks)
                            {
                                if(polyChunk is StripChunk)
                                {
                                    meshIndex++;
                                }
                                else if(polyChunk == anim.TextureChunk)
                                {
                                    if(atc.MeshData[meshIndex].Corners == null)
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    BufferMaterial mat = atc.MeshData[meshIndex].Material;

                                    mat.TextureIndex &= 0xC000;
                                    mat.TextureIndex += newTexID;
                                    break;
                                }
                            }
                        }
                    }

                }
            }

        }

        private void UpdateCamera(RenderContext context, float motionTimestamp)
        {
            if(!AnimateCamera)
            {
                CameraController?.Run(DeltaTimestamp);
                return;
            }

            EventMotion? cameraMotion = null;
            uint cameraMotionOffset = 0;
            foreach(EventMotion motion in CurrentScene.CameraAnimations)
            {
                if(motion.Animation == null)
                {
                    continue;
                }

                uint frameCount = motion.Animation.GetFrameCount();
                if(motionTimestamp > frameCount)
                {
                    cameraMotionOffset += frameCount;
                    continue;
                }

                cameraMotion = motion;
            }

            if(cameraMotion == null)
            {
                return;
            }

            Camera camera = context.Camera;

            camera.FarPlane = 100000f;
            camera.NearPlane = 1f;
            camera.Orbiting = false;

            if(cameraMotion.Value.Animation != null)
            {
                Frame frame = cameraMotion.Value.Animation.Keyframes[0].GetFrameAt(motionTimestamp);

                if(frame.Position != null)
                {
                    camera.Position = frame.Position.Value;
                }

                if(frame.Angle != null)
                {
                    camera.FieldOfView = 2 * MathF.Atan(MathF.Tan(frame.Angle.Value * 0.5f) * (480f / 640f));
                }

                if(frame.Target != null)
                {
                    Vector3 dir = Vector3.Normalize(frame.Target.Value - camera.Position);
                    float xAngle = MathF.Asin(dir.Y);
                    float yAngle = 0;
                    if(dir.Y is > (-1) and < 1)
                    {
                        yAngle = MathF.Atan2(-dir.X, -dir.Z);
                    }

                    camera.Rotation = new(xAngle, yAngle, -frame.Roll ?? 0);
                }
            }
        }

        private void UpdateTailsTails(RenderContext context)
        {
            if(TailsAnimations == null)
            {
                return;
            }

            foreach(TailsTailAnimation animation in TailsAnimations)
            {
                animation.Update(context, (float)DeltaTimestamp);
            }
        }
    }

    public static class EntityExtensions
    {
        public static void ProcessEntityAnimation(this EventEntry entity, RenderContext context, float timestamp)
        {
            Node model = entity.DisplayModel;

            if(!entity.Attributes.HasFlag(EventEntryAttribute.Scene_NoNodeAnimation) && entity.Animation != null)
            {
                Node[] nodes = model.GetAnimTreeNodes();
                for(int i = 0; i < nodes.Length; i++)
                {
                    if(entity.Animation.Keyframes.TryGetValue(i, out Keyframes? value))
                    {
                        Node node = nodes[i];

                        Frame frame = value.GetFrameAt(timestamp);

                        if(node.UseQuaternionRotation)
                        {
                            node.UpdateTransforms(
                                frame.Position,
                                frame.QuaternionRotation,
                                frame.Scale);
                        }
                        else
                        {
                            node.UpdateTransforms(
                                frame.Position,
                                frame.EulerRotation,
                                frame.Scale);
                        }
                    }
                }
            }

            if(!entity.Attributes.HasFlag(EventEntryAttribute.Scene_NoShapeAnimation) && entity.ShapeAnimation != null)
            {
                Node[] nodes = model.GetMorphTreeNodes();
                for(int i = 0; i < nodes.Length; i++)
                {
                    if(entity.ShapeAnimation.Keyframes.TryGetValue(i, out Keyframes? value))
                    {
                        Node node = nodes[i];
                        BufferMesh vertexMesh = node.Attach?.MeshData[0] ?? throw new NullReferenceException("Shape animation targets node with no mesh!");

                        if(vertexMesh.Vertices == null)
                        {
                            throw new NullReferenceException("Shape animation targets mesh with no vertices!");
                        }

                        Frame frame = value.GetFrameAt(timestamp);
                        if(frame.Vertex != null)
                        {
                            int len = Math.Min(frame.Vertex.Length, vertexMesh.Vertices.Length);
                            for(int j = 0; j < len; j++)
                            {
                                vertexMesh.Vertices[j].Position = frame.Vertex[j];
                            }
                        }

                        context.BufferMeshes(node.Attach.MeshData, true);
                    }
                }
            }

        }

    }

}
