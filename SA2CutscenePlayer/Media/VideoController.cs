using SA3D.Modeling.Mesh;
using SA3D.Rendering;
using SA3D.Rendering.UI;
using SA3D.Texturing;
using System;
using System.IO;
using MPEGFrame = PLMpegSharp.Container.Frame;
using MPEGPlayer = PLMpegSharp.Player;

namespace SA3D.SA2CutscenePlayer.Media
{
    public class VideoController : Canvas
    {
        private readonly TextureSet _meshTextures;

        private RenderContext? _context;
        private MPEGPlayer? _videoPlayer;
        private Texture _activeOutputTexture;
        private bool _overlay;
        private float _depth;
        public override bool EnableDepthCheck => true;

        public bool Paused { get; set; }
        public string VideoFileDirectory { get; set; }

        public VideoController(TextureSet meshTextures, string videoFileDirectory)
            : base(new(new[] { new ColorTexture(1, 1, new byte[4]) }))
        {
            _meshTextures = meshTextures;
            _activeOutputTexture = Textures.Textures[0];
            VideoFileDirectory = videoFileDirectory;
        }

        public void Reset()
        {
            Paused = false;
            if(_videoPlayer != null)
            {
                _videoPlayer.VideoDecodeCallback -= SetTextureData;
                _videoPlayer = null;
                _overlay = false;
                _depth = 0;
            }
        }


        public void StartMeshVideo(string filename, int textureIndex)
        {
            if(_videoPlayer != null)
            {
                return;
            }

            _activeOutputTexture = _meshTextures.Textures[textureIndex];

            InitVideo(filename);
        }

        public void StartUIVideo(string filename, float depth)
        {
            if(_videoPlayer != null)
            {
                return;
            }

            _overlay = true;
            _depth = depth;
            _activeOutputTexture = Textures.Textures[0];

            InitVideo(filename);
        }

        private void InitVideo(string filename)
        {
            _videoPlayer = new(Path.ChangeExtension(Path.Join(VideoFileDirectory, filename), ".sfd"))
            {
                AudioEnabled = false
            };
            _videoPlayer.VideoDecodeCallback += SetTextureData;

            MPEGFrame frame = _videoPlayer.DecodeVideo() 
                ?? throw new NullReferenceException("No frame decoded!");

            byte[] data = new byte[frame.Width * frame.Height * 4];
            Array.Fill(data, (byte)0xFF);
            frame.ToRGBA(data, frame.Width * 4);
            _activeOutputTexture.ReplaceData(frame.Width, frame.Height, data);

            _videoPlayer.Rewind();
        }

        private void SetTextureData(MPEGPlayer player, MPEGFrame? frame)
        {
            if(_context == null)
            {
                throw new InvalidOperationException("Context not set!");
            }

            if(frame != null)
            {
                frame.ToRGBA(_activeOutputTexture.Data, frame.Width * 4);
                _context?.ReloadTextureSetTexture(_activeOutputTexture);
            }
        }


        public void Update(RenderContext context, double delta)
        {
            if(Paused)
            {
                return;
            }

            _context = context;
            _videoPlayer?.Decode(delta);
            if(_videoPlayer?.HasEnded == true)
            {
                _videoPlayer.VideoDecodeCallback -= SetTextureData;
                _videoPlayer = null;
                _overlay = false;
            }

            _context = null;
        }

        protected override void Render(RenderContext context)
        {
            if(_overlay)
            {
                DrawSprite(new()
                {
                    ClipPos = _depth - 1,
                    Size = context.FloatViewport,
                    SourceBlendMode = BlendMode.One,
                    DestinationBlendMode = BlendMode.One
                });
            }
        }

    }
}
