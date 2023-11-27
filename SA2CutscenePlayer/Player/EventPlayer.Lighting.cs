using SA3D.SA2Event.Effects;
using SA3D.Rendering;
using SA3D.Modeling.Structs;
using System;
using System.Numerics;
using SA3D.Rendering.Structs;
using SA3D.SA2Event.Effects.Enums;

namespace SA3D.SA2CutscenePlayer.Player
{
    public partial class EventPlayer
    {
        private readonly Lighting[] _displayedLights = new Lighting[4];
        private readonly LightFadeMode[] _lightFadeModes = new LightFadeMode[4];

        private void ResetLighting()
        {
            Array.Clear(_displayedLights);
            Array.Fill(_lightFadeModes, LightFadeMode.Cut);
        }

        private void UpdateLighting(RenderContext context)
        {
            if(Cutscene.Effects == null)
            {
                return;
            }

            for(int i = 0; i < 2; i++)
            {
                ObjectLighting[] lightSet = Cutscene.Effects.Lighting[i];

                foreach(ObjectLighting light in lightSet)
                {
                    if(light.Frame <= Timestamp && light.Frame > _previousTimeStamp)
                    {
                        _displayedLights[i] = new(
                            -Vector3.Normalize(light.Direction),
                            1,
                            light.Diffuse,
                            light.Intensity,
                            light.Ambient);
                        _lightFadeModes[i] = light.Fade;
                        break;
                    }
                }

                if(_lightFadeModes[i] == LightFadeMode.FadeIn)
                {
                    ObjectLighting? from = null;
                    ObjectLighting? to = null;

                    foreach(ObjectLighting light in lightSet)
                    {
                        if(light.Frame > Timestamp && (to == null || light.Frame < to.Value.Frame))
                        {
                            to = light;
                        }

                        if(light.Frame <= Timestamp && (from == null || light.Frame > from.Value.Frame))
                        {
                            from = light;
                        }
                    }

                    if(from == null || to == null)
                    {
                        return;
                    }

                    float factor = 0;
                    if(from.Value.Frame != to.Value.Frame)
                    {
                        factor = ((float)Timestamp - from.Value.Frame) / (to.Value.Frame - from.Value.Frame);
                    }

                    _displayedLights[i] = new(
                        -Vector3.Normalize(Vector3.Lerp(from.Value.Direction, to.Value.Direction, factor)),
                        1,
                        Color.Lerp(from.Value.Diffuse, to.Value.Diffuse, factor),
                        (from.Value.Intensity * (1 - factor)) + (to.Value.Intensity * factor),
                        Color.Lerp(from.Value.Ambient, to.Value.Ambient, factor));
                }

            }

            context.SetLighting(_displayedLights);
        }

    }
}
