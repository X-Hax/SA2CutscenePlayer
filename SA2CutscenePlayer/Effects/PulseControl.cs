using SA3D.SA2Event.Effects;
using SA3D.SA2Event.Model;
using SA3D.Rendering;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Structs;
using System;
using System.Numerics;
using SA3D.Modeling.Mesh;

namespace SA3D.SA2CutscenePlayer.Effects
{
    public class PulseControl
    {
        // States:
        // 0 = Stay
        // 1 = Scale up
        // 2 = Scale down
        // 3 = Cuts to 0. duration must be 0. Only really useful when state 0 follows

        private static readonly (int state, int duration)[][] _pulseAnimation = new (int state, int duration)[][]
        {
            Array.Empty<(int, int)>(),
            new[] { (1, 2), (2, 3), (0, 3) },
            new[] { (1, 5), (3, 0) },
            new[] { (1, 10), (3, 0) },
            new[] { (1, 5), (2, 2), (0, 3) },
            new[] { (1, 12), (3, 0) },
            new[] { (1, 2), (3, 0), (1, 2), (3, 0), (0, 5) },
            new[] { (1, 4), (3, 0), (1, 4), (3, 0), (1, 4), (3, 0), (0, 10) },
            new[] { (1, 15), (3, 0), (0, 3) },
            new[] { (1, 13), (3, 0), (1, 5), (3, 0), (0, 3) },
            new[] { (1, 8), (2, 8) },
            new[] { (1, 12), (2, 12), (0, 5) },
            new[] { (1, 8), (2, 1) },
        };

        public struct PulseParticleInfo
        {
            public int unknown;
            public int pulseControl;
            public int textureID;
            public uint sceneIndex;
            public uint motionID;
            public float scale;
            public float frame;
        }

        public static float GetPulseSize(PulseParticleInfo particle, float timestamp)
        {
            if(particle.pulseControl > _pulseAnimation.Length)
            {
                return 0.0f;
            }

            (int state, int duration)[] animation = _pulseAnimation[particle.pulseControl];

            if(animation.Length == 0)
            {
                return 0.0f;
            }

            float localTimestamp = timestamp - particle.frame;
            int duration = animation[0].duration;
            int lastDuration = 0;

            int i = 0;
            bool scaledUpBefore = true;

            while(duration <= localTimestamp)
            {
                scaledUpBefore = animation[i].state == 1;

                if(++i >= animation.Length)
                {
                    i = 0;
                }

                lastDuration = duration;
                duration += animation[i].duration;
            }

            int nextState = animation[i].state;
            float temp = localTimestamp - lastDuration;

            if(nextState > 0)
            {
                if(nextState == 1)
                {
                    duration = 0;
                    localTimestamp = 1;
                }
                else if(nextState == 2)
                {
                    duration = 1;
                    localTimestamp = 0;
                }
            }
            else if(scaledUpBefore)
            {
                duration = 1;
                localTimestamp = 1;
            }
            else
            {
                duration = 0;
                localTimestamp = 0;
            }

            return particle.scale * (duration + ((localTimestamp - duration) * (temp / animation[i].duration)));
        }


        private readonly PulseParticleInfo[] _particleInfo = new PulseParticleInfo[64];

        public void Clear()
        {
            Array.Clear(_particleInfo);
        }

        public void Process(SimpleParticleEffect particle, uint sceneIndex)
        {
            int i = 0;
            while(_particleInfo[i].unknown != 0)
            {
                i++;
                if(i >= _particleInfo.Length)
                {
                    return;
                }
            }

            _particleInfo[i] = new()
            {
                unknown = (int)(particle.Unknown + 0.1f),
                textureID = (int)(particle.TextureID + 0.1f),
                pulseControl = (int)(particle.PulseControl + 0.1f),
                sceneIndex = sceneIndex,
                motionID = particle.MotionID,
                frame = particle.Frame,
                scale = particle.Scale
            };
        }

        public void FilterOrSomething(float unknown)
        {
            float end = unknown + 0.3f;
            if(end < 1.0)
            {
                return;
            }

            float start = unknown - 0.3f;
            for(int i = 0; i < _particleInfo.Length; i++)
            {
                float particleUnknown = _particleInfo[i].unknown;
                if(particleUnknown >= start && particleUnknown <= end)
                {
                    _particleInfo[i].unknown = 0;
                }
            }
        }

        public void Render(RenderContext context, ModelData eventData, int sceneIndex, float timestamp)
        {
            float sceneTimestamp = timestamp;
            for(int i = 0; i < sceneIndex; i++)
            {
                sceneTimestamp -= eventData.Scenes[i + 1].FrameCount;
            }

            Scene scene = eventData.Scenes[sceneIndex + 1];

            foreach(PulseParticleInfo particle in _particleInfo)
            {
                if(particle.unknown == 0)
                {
                    continue;
                }

                if(sceneIndex != particle.sceneIndex)
                {
                    FilterOrSomething(particle.unknown);
                    continue;
                }

                Vector3 position = scene.GetParticlePosition((int)particle.motionID, sceneTimestamp);
                float scale = GetPulseSize(particle, timestamp);

                if(scale == 0)
                {
                    return;
                }

                BufferMaterial material = new()
                {
                    UseAlpha = true,
                    SourceBlendMode = BlendMode.SrcAlpha,
                    DestinationBlendmode = BlendMode.One,
                    UseTexture = true,
                    TextureIndex = (uint)particle.textureID,
                    Diffuse = Color.ColorBlack,
                    Ambient = new Color(0xFF, 0xFF, 0xFF, 0x10),
                };

                (short width, short height) = eventData.TextureDimensions[particle.textureID];

                context.RenderBillBoard(
                    position,
                    new Vector2(width, height) * scale * 0.5f,
                    material);
            }
        }
    }
}
