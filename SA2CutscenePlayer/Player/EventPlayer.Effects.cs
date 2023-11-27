using SA3D.SA2Event.Effects;
using SA3D.Rendering;
using System;
using System.Numerics;
using SA3D.SA2CutscenePlayer.Player;
using SA3D.SA2CutscenePlayer.Effects;
using SA3D.SA2Event.Effects.Enums;

namespace SA3D.SA2CutscenePlayer.Player
{
    public partial class EventPlayer
    {
        public PulseControl PulseParticleController { get; }
        public LensFlare LensFlareController { get; }
        public ScreenEffectController ScreenEffectController { get; }
        public BlareEffectController BlareController { get; }
        public ReflectionHandler ReflectionHandler { get; }

        private Vector4 _currentForegroundColor;
        private Vector4 _targetForegroundColor;
        private Vector4 _currentBackgroundColor;
        private Vector4 _targetBackgroundColor;

        private void ResetEffects()
        {
            PulseParticleController.Clear();
            LensFlareController.Enabled = false;
            ScreenEffectController.Color = default;
            ScreenEffectController.ClearScreenTextures();
            BlareController.Clear();

            _currentForegroundColor = Vector4.Zero;
            _targetForegroundColor = Vector4.Zero;
            _currentBackgroundColor = Vector4.Zero;
            _targetBackgroundColor = Vector4.Zero;
        }

        private void UpdateEffects(RenderContext context)
        {
            UpdateBlare();
            UpdateParticles();
            UpdateScreenEffect(context);
        }

        private void UpdateBlare()
        {
            if(Cutscene.Effects == null)
            {
                return;
            }

            foreach(BlareEffect blareEffect in Cutscene.Effects.BlareEffects)
            {
                if(blareEffect.Frame > Timestamp
                    || blareEffect.Frame <= CurrentSceneFrameOffset)
                {
                    continue;
                }

                BlareController.CreateGhostTracker(blareEffect);
            }

            if(Math.Floor(_previousTimeStamp) < Math.Floor(Timestamp))
            {
                BlareController.AddGhosts(Cutscene.ModelData, 0, (float)Timestamp);
                BlareController.AddGhosts(Cutscene.ModelData, SceneIndex + 1, (float)Timestamp);
                BlareController.UpdateGhosts();
            }
        }

        private void UpdateParticles()
        {
            if(Cutscene.Effects == null)
            {
                return;
            }

            for(int i = 0; i < Cutscene.Effects.Particles.Length; i++)
            {
                SimpleParticleEffect particle = Cutscene.Effects.Particles[i];
                if(particle.Frame > Timestamp
                    || particle.Frame <= CurrentSceneFrameOffset)
                {
                    continue;
                }

                Vector3 particlePosition = CurrentScene.GetParticlePosition(particle.MotionID, SceneTimestamp);

                switch(particle.Type)
                {
                    case SimpleParticleType.EnableSun:
                        LensFlareController.Enabled = true;
                        LensFlareController.FlarePosition = particlePosition;
                        break;
                    case SimpleParticleType.DisableSun:
                        LensFlareController.Enabled = false;
                        break;
                    case SimpleParticleType.PulseStart:
                        PulseParticleController.Process(particle, (uint)SceneIndex);
                        break;
                    case SimpleParticleType.PulseEnd:
                        PulseParticleController.FilterOrSomething(particle.Unknown);
                        break;
                }
            }
        }

        private void UpdateScreenEffect(RenderContext context)
        {
            if(Cutscene.Effects == null)
            {
                return;
            }

            foreach(ScreenEffect effect in Cutscene.Effects.ScreenEffects)
            {
                if(effect.Frame > Timestamp
                    || effect.Frame <= _previousTimeStamp)
                {
                    continue;
                }

                switch(effect.Type)
                {
                    case ScreenEffectType.ForegroundFadeIn:
                        _targetForegroundColor = effect.Color.FloatVector;
                        break;
                    case ScreenEffectType.ForegroundCutIn:
                        _targetForegroundColor = _currentForegroundColor = effect.Color.FloatVector;
                        break;
                    case ScreenEffectType.TextureFadeIn:
                    case ScreenEffectType.TextureCutIn:
                        ScreenEffectController.CreateScreenTexture(effect, Cutscene.ModelData.TextureDimensions);
                        break;
                    case ScreenEffectType.BackgroundFadeIn:
                        _targetBackgroundColor = effect.Color.FloatVector;
                        break;
                    case ScreenEffectType.BackgroundCutIn:
                        _targetBackgroundColor = _currentBackgroundColor = effect.Color.FloatVector;
                        break;
                }
            }

            float distance = (float)DeltaTimestamp * 2 * (3f / 255f);
            _currentForegroundColor = FadeUtil.FadeColor(_currentForegroundColor, _targetForegroundColor, distance);
            _currentBackgroundColor = FadeUtil.FadeColor(_currentBackgroundColor, _targetBackgroundColor, distance);

            ScreenEffectController.Color = new(_currentForegroundColor);
            ScreenEffectController.UpdateScreenTextures((float)Timestamp);
            context.BackgroundColor = new(_currentBackgroundColor);
        }
    }
}
