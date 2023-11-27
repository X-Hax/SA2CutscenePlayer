using SA3D.SA2Event.Effects;
using SA3D.Rendering;
using SA3D.Rendering.UI;
using SA3D.Modeling.Structs;
using SA3D.Texturing;
using System.Collections.Generic;
using System.Numerics;
using SA3D.SA2Event.Effects.Enums;

namespace SA3D.SA2CutscenePlayer.Effects
{
    public class ScreenEffectController : Canvas
    {
        private class ScreenTexture
        {
            public bool FadeIn { get; }
            public bool FadeOut { get; }
            public uint Frame { get; }
            public uint FrameTime { get; }

            public ushort TextureID { get; }
            public Vector2 Position { get; }
            public Vector2 Size { get; }

            public float Transparency { get; set; }

            public ScreenTexture(bool fadeIn, bool fadeOut, uint frame, uint frameTime, ushort textureID, Vector2 position, Vector2 size)
            {
                FadeIn = fadeIn;
                FadeOut = fadeOut;
                Frame = frame;
                FrameTime = frameTime;
                TextureID = textureID;
                Position = position;
                Size = size;
                Transparency = 1;
            }
        }

        public Color Color { get; set; }

        private readonly List<ScreenTexture> _screenTextures;

        public ScreenEffectController(TextureSet textures) : base(textures)
        {
            _screenTextures = new();
        }

        protected override void Render(RenderContext context)
        {
            foreach(ScreenTexture screenTex in _screenTextures)
            {
                Vector2 size = screenTex.Size / 480 * context.Viewport.Height;
                Vector2 position =
                    (screenTex.Position / 480 * context.Viewport.Height)
                    + (context.FloatViewport * 0.5f);


                DrawSprite(new()
                {
                    TextureIndex = screenTex.TextureID,
                    Position = position,
                    Size = size,
                    Color = new(1, 1, 1, screenTex.Transparency)
                });
            }

            if(Color.Alpha > 0)
            {
                DrawRectangle(new()
                {
                    Color = Color,
                    Size = new(context.Viewport.Width, context.Viewport.Height)
                });
            }
        }

        public void UpdateScreenTextures(float timestamp)
        {
            for(int i = 0; i < _screenTextures.Count; i++)
            {
                ScreenTexture screenTex = _screenTextures[i];
                float frameOffset = timestamp - screenTex.Frame;
                if(screenTex.FrameTime <= frameOffset)
                {
                    _screenTextures.RemoveAt(i);
                    i--;
                    continue;
                }

                screenTex.Transparency = 1;

                if(screenTex.FadeIn && frameOffset < 30)
                {
                    screenTex.Transparency = frameOffset / 30f;
                }
                else if(screenTex.FadeOut && frameOffset >= screenTex.FrameTime - 30)
                {
                    screenTex.Transparency = (screenTex.Frame + screenTex.FrameTime - timestamp) / 30f;
                }
            }
        }

        public void ClearScreenTextures()
        {
            _screenTextures.Clear();
        }

        public void CreateScreenTexture(ScreenEffect effect, (short w, short h)[] textureSizes)
        {
            _screenTextures.Add(new ScreenTexture(
                effect.Type == ScreenEffectType.TextureFadeIn,
                effect.FadeOut,
                effect.Frame,
                effect.FrameTime,
                effect.TextureID,
                new(effect.PositionX, effect.PositionY),
                new(effect.Width * textureSizes[effect.TextureID].w,
                    effect.Height * textureSizes[effect.TextureID].h)));
        }
    }
}
