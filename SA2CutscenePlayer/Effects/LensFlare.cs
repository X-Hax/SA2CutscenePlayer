using SA3D.Rendering;
using SA3D.Rendering.UI;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.Structs;
using SA3D.Texturing;
using System;
using System.Numerics;

namespace SA3D.SA2CutscenePlayer.Effects
{
    public class LensFlare : Canvas
    {
        private readonly struct FlareInfo
        {
            public readonly int TextureID;
            public readonly float OffsetFactor;
            public readonly float Scale;
            public readonly float DistanceScale;
            public readonly float Transparency;
            public readonly float ClipPos;

            public FlareInfo(int textureID, float offsetFactor, float scale, float distanceScale, float transparency, float clipPos)
            {
                TextureID = textureID;
                OffsetFactor = offsetFactor;
                Scale = scale;
                DistanceScale = distanceScale;
                Transparency = transparency;
                ClipPos = clipPos;
            }
        }

        private static readonly FlareInfo[] _flareInfoData = new FlareInfo[]
        {
            new( 67, 0.1f, 0.8f, 0.0f, 0.7f, -11.0f),
            new( 68, 0.4f, 0.6f, 0.5f, 0.7f, -11.1f),
            new( 69, 0.6f, 0.8f, 0.6f, 0.7f, -11.2f),
            new( 67, 0.8f, 1.2f, 0.6f, 0.4f, -11.3f),
            new( 69, 1.0f, 1.2f, -0.3f, 0.7f, -11.4f),
            new( 70, 1.2f, 0.7f, 1.0f, 0.7f, -11.5f),
            new( 68, 1.4f, 1.7f, 0.0f, 0.7f, -11.6f),
            new( 71, 1.5f, 0.6f, 1.0f, 0.7f, -11.7f),
            new( 72, 1.8f, 1.5f, 0.0f, 0.7f, -11.8f),
            new( 68, 0.0f, 4.0f, 2.0f, 0.4f, -11.9f),
        };

        public bool Enabled { get; set; }
        public Vector3 FlarePosition { get; set; }

        public override bool EnableDepthCheck => true;

        public LensFlare(TextureSet textures) : base(textures) { }

        protected override void Render(RenderContext context)
        {
            if(!Enabled)
            {
                return;
            }

            void Draw(int textureIndex, Vector2 position, Vector2 scale, Color color, float clipPos)
            {
                float dimensions = 100f / 480f * context.Viewport.Height;
                Vector2 pixelScale = new Vector2(dimensions) * scale;
                Vector2 pixelPosition = position - (pixelScale * 0.5f);

                DrawSprite(new()
                {
                    TextureIndex = textureIndex,
                    Position = pixelPosition,
                    ClipPos = clipPos,
                    Size = pixelScale,
                    Color = color,
                    SourceBlendMode = BlendMode.SrcAlpha,
                    DestinationBlendMode = BlendMode.One
                });
            }

            Vector3 direction = Vector3.Normalize(FlarePosition - context.Camera.Realposition);
            float dot = Vector3.Dot(context.Camera.Forward, direction);

            if(dot <= 0)
            {
                return;
            }

            Vector4 position = Vector4.Transform(FlarePosition, context.Camera.GetMVPMatrix(Matrix4x4.Identity));
            if(position.W != 0)
            {
                position /= position.W;
            }

            Matrix4x4.Invert(ProjectionMatrix, out Matrix4x4 toCanvasSpace);
            position = Vector4.Transform(position, toCanvasSpace);

            Vector2 canvasPosition = new(position.X, position.Y);

            if(dot > 0.99f)
            {
                float scaleTransparency = (dot - 0.98f) * 50;
                scaleTransparency *= scaleTransparency;

                Color color = new(1, 1, 1, 0.90f * scaleTransparency);
                Vector2 scale = new(scaleTransparency * 20);

                Draw(86, canvasPosition, scale, color, -12);
            }

            Draw(66, canvasPosition, Vector2.One, Color.ColorWhite, -13);

            float transparency = 0;
            if(dot > 0.6f)
            {
                transparency = (dot - 0.6f) * 3;
                transparency *= transparency;
                transparency *= transparency;
            }

            float visibility = 1.0f - transparency;

            Vector2 viewport = new(context.Viewport.Width, context.Viewport.Height);
            Vector2 offCenter = (viewport * 0.5f) - canvasPosition;

            foreach(FlareInfo info in _flareInfoData)
            {
                Draw(
                    info.TextureID,
                    (info.OffsetFactor * offCenter) + canvasPosition,
                    new(Math.Max(0, (1.0f - (info.DistanceScale * visibility)) * info.Scale)),
                    new(1, 1, 1, MathF.Min(1, info.Transparency * transparency)),
                    info.ClipPos);
            }
        }


    }
}
