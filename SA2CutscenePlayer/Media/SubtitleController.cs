using FontStashSharp;
using OpenTK.Graphics.OpenGL4;
using SA3D.SA2Event.Language;
using SA3D.Rendering;
using SA3D.Rendering.Buffer;
using SA3D.Rendering.Shaders;
using SA3D.Rendering.UI;
using SA3D.Texturing;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace SA3D.SA2CutscenePlayer.Media
{
    public class SubtitleController : Canvas
    {
        private readonly SubtitleFile? _subtitles;

        public uint EventIndex { get; set; }
        public int DisplayIndex { get; set; }
        public int FontSize { get; set; } = 30;

        private readonly TextureFrameBuffer _fbo;
        private readonly Shader _outlineShader;

        public bool Paused { get; set; }

        public SubtitleController(SubtitleFile? subtitles, uint eventID) : base(new(Array.Empty<Texture>()))
        {
            _subtitles = subtitles;
            EventIndex = eventID;
            DisplayIndex = -1;

            FontSystem fontSystem = new(new()
            {
                FontResolutionFactor = 2,
                KernelWidth = 2,
                KernelHeight = 2,
                PremultiplyAlpha = false
            });

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "SA3D.SA2CutscenePlayer.Media.MPLUSRounded1c-Bold.ttf";
            using(Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new NullReferenceException())
            {
                fontSystem.AddFont(stream);
            }

            FontManager = new(fontSystem);

            string fragmentShader = "";
            resourceName = "SA3D.SA2CutscenePlayer.Media.FontOutline.frag";
            using(Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new NullReferenceException())
            {
                fragmentShader = new StreamReader(stream).ReadToEnd();
            }

            _fbo = new(false);
            _outlineShader = new(
                "Subtitle Outline",
                VertexShaders.GetScreen(),
                fragmentShader);
        }

        protected override void Initialize()
        {
            _outlineShader.Compile();
        }

        protected override void Render(RenderContext context)
        {
            if((!Paused && DisplayIndex == -1) || FontManager == null)
            {
                return;
            }

            int previousFrameBuffer = GL.GetInteger(GetPName.FramebufferBinding);

            _fbo.Generate(context.Viewport);

            _fbo.BindFrameBuffer();
            GL.ClearColor(Color.Transparent);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float realFontSize = FontSize / 480f * context.Viewport.Height;
            if(DisplayIndex != -1)
            {
                string text;

                try
                {
                    text = _subtitles?.Texts[EventIndex][DisplayIndex].Text ?? $"SUBTITLE #{DisplayIndex}";
                }
                catch
                {
                    text = $"SUBTITLE #{DisplayIndex}";
                }

                string[] lines = text.Split('\n');
                float heightOffset = lines.Length + 0.5f;
                foreach(string line in lines)
                {
                    float length = FontManager.FontSystem.GetFont(realFontSize).MeasureString(line).X;
                    float x = (context.Viewport.Width - length) * 0.5f;
                    float y = context.Viewport.Height - (heightOffset * realFontSize);
                    heightOffset--;

                    DrawText(line, realFontSize, new(x, y));
                }

            }

            if(Paused)
            {
                const string pausedtext = "Paused  II";
                float length = FontManager.FontSystem.GetFont(realFontSize).MeasureString(pausedtext).X;
                DrawText(pausedtext, realFontSize, new(context.Viewport.Width - length - (realFontSize * 0.5f), realFontSize * 0.5f));
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFrameBuffer);
            _outlineShader.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            _fbo.BindColorTexture();
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Blit.Render();
        }
    }
}
