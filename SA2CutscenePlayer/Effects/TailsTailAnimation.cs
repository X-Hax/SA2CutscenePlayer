using SA3D.Modeling.Mesh;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Rendering;
using System;
using System.Numerics;
using static SA3D.Common.MathHelper;

namespace SA3D.SA2CutscenePlayer.Effects
{
    public class TailsTailAnimation
    {
        private readonly BufferVertex[] _sourceVertices;
        private readonly BufferVertex[] _targetVertices;
        private readonly float _other1;
        private readonly float _other2;
        private float _timestamp;
        private float _speed;

        private static readonly float[] _speedMap = new float[]
        {
            BAMSToRad(0x300),
            BAMSToRad(0x220),
            BAMSToRad(0x2C0),
            BAMSToRad(0x280),
            BAMSToRad(0x140),
            BAMSToRad(0x1C0),
            BAMSToRad(0x240),
            BAMSToRad(0x2A0),
        };

        public Attach Mesh { get; }

        public TailsTailAnimation(Attach mesh, bool other)
        {
            Mesh = mesh;
            if(Mesh.MeshData.Length == 0)
            {
                throw new ArgumentException("Mesh has no buffer data!");
            }

            BufferMesh bufferMesh = Mesh.MeshData[0];
            if(bufferMesh.Vertices == null)
            {
                throw new ArgumentException("Mesh has no vertex data!");
            }

            _targetVertices = bufferMesh.Vertices;
            _sourceVertices = (BufferVertex[])_targetVertices.Clone();

            _other1 = other ? -2 : 2;
            _other2 = other ? -0.8f : 0.8f;
            _speed = _speedMap[0];
        }

        private void AnimateVertices()
        {
            float timestamp = _timestamp % float.Tau;

            float distance = Mesh.MeshBounds.Radius * 2;
            float time = distance * (timestamp % float.Pi) / float.Pi;
            float timeInv = distance - time;
            // 0 >= time|Inv >= distance

            float timeHP = time * HalfPi;
            float timeInvHP = timeInv * HalfPi;

            for(int i = 0; i < _sourceVertices.Length; i++)
            {
                Vector3 pos = _sourceVertices[i].Position;
                Vector3 newPos = pos;

                if(pos.Y >= 0.0)
                {
                    float vAngle;
                    float vSin;
                    float vCos;
                    int dir;

                    if(timeHP > pos.Y) // sway away from center
                    {
                        vAngle = pos.Y / timeHP * float.Pi;
                        vSin = MathF.Sin(vAngle) * time * 0.5f;
                        vCos = -(MathF.Cos(vAngle) - 1.0f) * time * 0.5f;
                        dir = 1;
                    }
                    else // sway to center
                    {
                        vAngle = float.Tau - ((pos.Y - timeHP) / timeInvHP * float.Pi);
                        vSin = MathF.Sin(vAngle) * timeInv * 0.5f;
                        vCos = time - ((MathF.Cos(vAngle) - 1.0f) * timeInv * 0.5f);
                        dir = -1;
                    }

                    float vAngle2 = (HalfPi + (vAngle * dir)) * 0.5f;
                    float vSin2 = MathF.Sin(vAngle2);
                    float vCos2 = MathF.Cos(vAngle2);

                    float heightFactor = vCos2 * pos.X * _other1;

                    if(timestamp >= float.Pi)
                    {
                        vSin = -vSin;
                        vSin2 = -vSin2;
                    }

                    newPos.Y = (vCos - (vSin2 * dir * heightFactor)) * 1.15f;
                    newPos.X += (vSin - (vCos2 * heightFactor)) * _other2;
                }

                _targetVertices[i].Position = newPos;
            }
        }

        public void Reset()
        {
            _timestamp = 0;
            _speed = _speedMap[0];
        }

        public void Update(RenderContext context, float delta)
        {
            AnimateVertices();

            _timestamp += delta * _speed * 2;

            int speedIndex = (int)Math.Floor(_timestamp / float.Tau) % _speedMap.Length;
            _speed = _speedMap[speedIndex];

            _timestamp %= float.Tau * _speedMap.Length;


            context.BufferMeshes(Mesh.MeshData, true);
        }
    }
}
