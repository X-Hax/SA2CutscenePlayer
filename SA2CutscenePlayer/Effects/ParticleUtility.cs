using SA3D.Modeling.Animation;
using SA3D.SA2Event.Model;
using System.Numerics;

namespace SA3D.SA2CutscenePlayer.Effects
{
    internal static class ParticleUtility
    {
        public static Vector3 GetParticlePosition(this Scene scene, int index, float timestamp)
        {
            if(index >= scene.ParticleMotions.Count)
            {
                return default;
            }

            Motion? particleMotion = scene.ParticleMotions[index];
            if(particleMotion == null)
            {
                return default;
            }

            Frame frame = particleMotion.Keyframes[0].GetFrameAt(timestamp);

            return frame.Position ?? default;
        }
    }
}
