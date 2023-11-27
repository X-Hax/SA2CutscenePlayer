using OpenTK.Graphics.OpenGL4;
using SA3D.SA2Event.Model;
using SA3D.Rendering;
using SA3D.Rendering.Buffer;
using SA3D.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh;

namespace SA3D.SA2CutscenePlayer.Effects
{
    public class ReflectionHandler
    {
        private readonly struct ReflectionSetup
        {
            public Matrix4x4 ReflectionMatrix { get; }
            public BufferMesh ReflectionPlane { get; }

            public ReflectionSetup(Matrix4x4 reflectionMatrix, BufferMesh reflectionPlane)
            {
                ReflectionMatrix = reflectionMatrix;
                ReflectionPlane = reflectionPlane;
            }
        }

        private readonly ReflectionSetup[] _reflections;
        private readonly OITBuffer _reflectionOITbuffer;
        private readonly TextureFrameBuffer _fbo;

        private static Shader CompShader => Shaders.MeshComposite;

        public ReflectionHandler(List<Reflection> reflections)
        {
            List<ReflectionSetup> reflectionData = new();

            foreach(Reflection reflection in reflections)
            {
                if(reflection.Transparency == 0)
                {
                    continue;
                }

                Vector3 position = (reflection.Vertex1 + reflection.Vertex2 + reflection.Vertex3 + reflection.Vertex4) * 0.25f;
                Vector3 d1 = reflection.Vertex2 - reflection.Vertex1;
                Vector3 d2 = reflection.Vertex3 - reflection.Vertex1;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(d1, d2));

                float xzLen = (normal * new Vector3(1, 0, 1)).Length();
                float xAngle = MathF.Acos(xzLen);
                if(normal.Y > 0.0)
                {
                    xAngle = -xAngle;
                }

                float yAngle = MathF.Acos(normal.Z / xzLen);
                if(normal.X > 0.0)
                {
                    yAngle = -yAngle;
                }

                Matrix4x4 matrix =
                    Matrix4x4.CreateTranslation(-position)
                    * Matrix4x4.CreateRotationY(-yAngle)
                    * Matrix4x4.CreateRotationX(-xAngle)
                    * Matrix4x4.CreateScale(1, 1, -1)
                    * Matrix4x4.CreateRotationX(xAngle)
                    * Matrix4x4.CreateRotationY(yAngle)
                    * Matrix4x4.CreateTranslation(position);

                BufferMesh bufferMesh = new(
                    new BufferVertex[]
                    {
                        new(reflection.Vertex1, 0),
                        new(reflection.Vertex2, 1),
                        new(reflection.Vertex3, 2),
                        new(reflection.Vertex4, 3),
                    },
                    new()
                    {
                        UseAlpha = true,
                        SourceBlendMode = BlendMode.SrcAlpha,
                        DestinationBlendmode = BlendMode.SrcAlphaInverted,
                        Diffuse = new(1, 1, 1, reflection.Transparency / 100f)
                    },
                    new BufferCorner[]
                    {
                        new(0), new(1), new(2), new(1), new(2), new(3)
                    },
                    null,
                    false,
                    false,
                    false,
                    false,
                    0,
                    0);

                reflectionData.Add(new(matrix, bufferMesh));
            }

            _reflections = reflectionData.ToArray();
            _reflectionOITbuffer = new();
            _fbo = new(false);
        }

        public void Initialize(RenderContext context)
        {
            context.BufferMeshes(_reflections.Select(x => x.ReflectionPlane).ToArray());
        }

        public void Render(RenderContext context, Scene baseScene, Scene scene)
        {
            _reflectionOITbuffer.Setup(context.Viewport);
            _fbo.Generate(context.Viewport);
            GL.ClearColor(0, 0, 0, 0);
            Shader previousShader = context.MeshShader;

            foreach(ReflectionSetup item in _reflections)
            {
                _reflectionOITbuffer.Use();
                _reflectionOITbuffer.Reset();
                context.SetMeshShader(previousShader);
                context.Camera.CustomWorldMatrix = item.ReflectionMatrix;

                foreach(EventEntry entity in baseScene.Entries)
                {
                    if(entity.Attributes.HasFlag(EventEntryAttribute.Reflection))
                    {
                        context.RenderModel(entity.DisplayModel);
                    }
                }

                foreach(EventEntry entity in scene.Entries)
                {
                    if(entity.Attributes.HasFlag(EventEntryAttribute.Reflection))
                    {
                        context.RenderModel(entity.DisplayModel);
                    }
                }

                _reflectionOITbuffer.BindComposite(CompShader);
                context.OITBuffer.Use();

                context.Camera.CustomWorldMatrix = Matrix4x4.Identity;
                context.SetMeshShader(CompShader);
                context.RenderMesh(item.ReflectionPlane, new(Matrix4x4.Identity, context.Camera.GetMVPMatrix(Matrix4x4.Identity)));
            }

            GL.ClearColor(context.BackgroundColor.SystemColor);
            context.SetMeshShader(previousShader);
        }
    }
}
