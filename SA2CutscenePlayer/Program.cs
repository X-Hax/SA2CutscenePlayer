using Microsoft.Toolkit.HighPerformance;
using OpenTK.Windowing.Common.Input;
using SA3D.Archival;
using SA3D.SA2Event;
using SA3D.SA2Event.Language;
using SA3D.Rendering;
using SA3D.Rendering.Input;
using SA3D.SA2CutscenePlayer.Player;
using SA3D.Texturing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using IconImage = OpenTK.Windowing.Common.Input.Image;
using SA3D.Rendering.Shaders;

namespace SA3D.SA2CutscenePlayer
{
    public static class Program
    {
        private static readonly int[] _upgradeMaskCharacterMapping = new int[]
        {
            0, 0, 0, 0, 0, 0,
            1, 1, 1, 1,
            2, 2, 2, 2, 2,
            -1,
            3, 3, 3, 3,
            4, 4, 4, 4, 4,
            5, 5, 5, 5
        };

        public static void Main(string[] args)
        {
            if(!Help(args))
            {
                return;
            }

            EventPlayer player;
            try
            {
                player = args[0].ToLower() == "manual" 
                    ? RunManual(args) 
                    : RunSimple(args);
            }
            catch(ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            RenderContext context = new(new(800, 600))
            {
                BackgroundColor = Modeling.Structs.Color.ColorBlack
            };
            DebugController debugController = new(context);
            player.CameraController = new(context.Input, context.Camera);

            context.OnInitialize += player.Load;
            context.OnUpdate += (c, d) => debugController.Run(d);
            context.OnUpdate += player.Update;
            context.OnRender += player.Render;

            context.AddCanvas(player.LensFlareController);
            if(player.VideoController != null)
            {
                context.AddCanvas(player.VideoController);
            }

            context.AddCanvas(player.ScreenEffectController);
            if(player.SubtitleController != null)
            {
                context.AddCanvas(player.SubtitleController);
            }

            context.AddCanvas(debugController.Overlay);

            RenderWindow window = new(context, "SA3D - SA2 Cutscene Player", GetIcon());

            window.Move += (e) => player.Pause();
            window.Resize += (e) => player.Pause();

            window.Run();
        }

        private static bool Help(string[] arguments)
        {
            if(arguments.Length == 0 || arguments[0] == "?" || arguments[0].StartsWith("-"))
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "SA3D.SA2CutscenePlayer.cmdhelp.txt";
                using(Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new NullReferenceException()) // error cannot happen tho
                {
                    Console.WriteLine(new StreamReader(stream).ReadToEnd());
                }

                return false;
            }

            return true;
        }

        private static void GetOptionalArguments(
            string[] arguments,
            out EventLanguage language,
            out bool englishAudio,
            out float musicVolume,
            out float sfxVolume,
            out float voiceVolume,
            out string upgradeMask)
        {
            language = EventLanguage.English;
            englishAudio = true;
            musicVolume = 0.6f;
            sfxVolume = 0.4f;
            voiceVolume = 1;
            upgradeMask = "0";

            for(int i = 2; i < arguments.Length; i++)
            {
                float getFloat(string name)
                {
                    i++;
                    return !float.TryParse(arguments[i], out float volume)
                        ? throw new ArgumentException($"Please enter a valid floating point number for the {name} volume (e.g. 0.95)")
                        : float.Clamp(volume, 0, 10);
                }

                switch(arguments[i].ToLower())
                {
                    case "-sl":
                    case "--sub-language":
                        i++;
                        language = (EventLanguage)arguments[i].ToUpper()[0];
                        if(arguments[i].Length != 1 || !Enum.IsDefined(language))
                        {
                            throw new ArgumentException("Language invalid! Please see -help for valid languages");
                        }

                        break;
                    case "-al":
                    case "--audio-language":
                        i++;
                        char audioLanguageCode = arguments[i].ToUpper()[0];
                        if(arguments[i].Length != 1 || !(audioLanguageCode == 'E' || audioLanguageCode == 'J'))
                        {
                            throw new ArgumentException("Audio language invalid! Please see -help for valid audio languages");
                        }

                        englishAudio = audioLanguageCode == 'E';
                        break;
                    case "-mv":
                    case "--music-volume":
                        musicVolume = getFloat("music");
                        break;
                    case "-sv":
                    case "--sfx-volume":
                        sfxVolume = getFloat("SFX");
                        break;
                    case "-vv":
                    case "--voice-volume":
                        voiceVolume = getFloat("voice");
                        break;
                    case "-ug":
                    case "--upgrades":
                        i++;
                        upgradeMask = arguments[i].ToUpper();
                        if(upgradeMask.Length is not 1 and not 6 and not 29)
                        {
                            throw new ArgumentException("Upgrade mask has to be either 1, 6 or 29 characters long!");
                        }

                        if(upgradeMask.Count('X') + upgradeMask.Count('0') != upgradeMask.Length)
                        {
                            throw new ArgumentException("Upgrade mask only permits 'X' and '0'!");
                        }

                        break;
                }
            }
        }

        private static void SetUpgrades(string upgradeMask, EventPlayer player)
        {
            switch(upgradeMask.Length)
            {
                case 6:
                    for(int i = 0; i < 29; i++)
                    {
                        int characterIndex = _upgradeMaskCharacterMapping[i];
                        if(characterIndex == -1)
                        {
                            player.ToggleUpgrade(i, false);
                            continue;
                        }

                        player.ToggleUpgrade(i, upgradeMask[characterIndex] == 'X');
                    }

                    break;
                case 29:
                    for(int i = 0; i < 29; i++)
                    {
                        player.ToggleUpgrade(i, upgradeMask[i] == 'X');
                    }

                    break;
                default:
                    player.ToggleAllUpgrades(upgradeMask[0] == 'X');
                    break;
            }
        }

        private static EventPlayer RunSimple(string[] arguments)
        {
            if(arguments.Length < 2)
            {
                throw new ArgumentException("Filepath and event ID required!");
            }

            GetOptionalArguments(arguments,
                out EventLanguage language,
                out bool englishAudio,
                out float musicVolume,
                out float sfxVolume,
                out float voiceVolume,
                out string upgradeMask);

            string rootPath = arguments[0];
            if(!Directory.Exists(rootPath))
            {
                throw new ArgumentException("Specified path does not exist!");
            }

            string basePath = Path.Join(rootPath, "resource\\gd_PC");

            if(!int.TryParse(arguments[1], out int eventID))
            {
                throw new ArgumentException("Event ID must be a number!");
            }
            else if(eventID is < 0 or > 9999)
            {
                throw new ArgumentException("Event id cannot contain more than 4 digits!");
            }

            string eventPath = Path.Join(basePath, $"event\\e{eventID:D4}.prs");
            if(!File.Exists(eventPath))
            {
                throw new ArgumentException($"Event file \"{Path.GetFileName(eventPath)}\" not found!");
            }

            string effectTexturePath = Path.Join(basePath, $"efftex_common.prs");
            if(!File.Exists(effectTexturePath))
            {
                throw new ArgumentException($"Effect textures \"{Path.GetFileName(effectTexturePath)}\" not found!");
            }

            char? subtitleType = null;
            if(eventID < 100)
            {
                subtitleType = 'H';
            }
            else if(eventID < 200)
            {
                subtitleType = 'D';
            }
            else if(eventID < 300)
            {
                subtitleType = 'L';
            }

            string? subtitleFilePath = null;
            if(subtitleType != null)
            {
                subtitleFilePath = Path.Join(basePath, $"event\\evmes{subtitleType}{(language == EventLanguage.Japanese ? '0' : (char)language)}.prs");
                if(!File.Exists(effectTexturePath))
                {
                    throw new ArgumentException($"Subtitle file \"{Path.GetFileName(subtitleFilePath)}\" not found!");
                }
            }

            string videoPath = basePath;

            string adxPath = Path.Join(basePath, $"ADX");
            if(!Directory.Exists(adxPath))
            {
                throw new ArgumentException($"ADX directory doesnt exist!");
            }

            string? sfxPath = Path.Join(basePath, "event\\MLT");
            if(!Directory.Exists(sfxPath))
            {
                throw new ArgumentException($"Sound effect path (event\\MLT) doesnt exist!");
            }

            sfxPath = Path.Join(sfxPath, $"e{eventID:D4}");
            if(!Directory.Exists(sfxPath))
            {
                sfxPath = null;
            }

            string voiceAudioPath = Path.Join(basePath, $"event_adx{(englishAudio ? "_e" : "")}.afs");
            if(!File.Exists(voiceAudioPath))
            {
                throw new ArgumentException($"Voice audio file \"{Path.GetFileName(voiceAudioPath)}\" not found!");
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Event cutscene = Event.ReadFromFiles(eventPath);

            TextureSet textures = cutscene.TextureArchive?.ToTextureSet() ?? throw new ArgumentException($"Event has no texture file!");
            TextureSet effectTextures = Archive.ReadArchiveFromFile(effectTexturePath).ToTextureSet();
            SubtitleFile? subtitles = subtitleFilePath == null ? null : SubtitleFile.ReadFromFile(
                    subtitleFilePath,
                    cutscene.Type.GetSubtitleImageBase(),
                    cutscene.Type.GetBigEndian(),
                    true,
                    language == EventLanguage.Japanese ? Encoding.GetEncoding("shift-jis") : Encoding.Latin1);

            EventPlayer result = new(cutscene, textures, effectTextures, subtitles, (uint)eventID, language, videoPath, adxPath, sfxPath, voiceAudioPath);

            result.AudioController.VoiceVolume = voiceVolume;
            result.AudioController.ADXVolume = musicVolume;
            result.AudioController.SoundEffectVolume = sfxVolume;

            SetUpgrades(upgradeMask, result);

            return result;
        }

        private static EventPlayer RunManual(string[] arguments)
        {
            if(arguments.Length < 3)
            {
                throw new ArgumentException("Event filepath and effect texture filepath required!");
            }

            string eventPath = arguments[1];
            if(!File.Exists(eventPath))
            {
                throw new ArgumentException($"Event file \"{Path.GetFileName(eventPath)}\" not found!");
            }

            string texturePath = eventPath.Replace(".prs", "texture.prs");
            if(!File.Exists(texturePath))
            {
                throw new ArgumentException($"Event textures \"{Path.GetFileName(texturePath)}\" not found!");
            }

            string effectTexturePath = arguments[2];
            if(!File.Exists(eventPath))
            {
                throw new ArgumentException($"Texture file \"{Path.GetFileName(effectTexturePath)}\" not found!");
            }

            GetOptionalArguments(arguments,
               out EventLanguage language,
               out bool englishAudio,
               out float musicVolume,
               out float sfxVolume,
               out float voiceVolume,
               out string upgradeMask);

            Event cutscene = Event.ReadFromFiles(eventPath);

            TextureSet textures = cutscene.TextureArchive?.ToTextureSet() ?? throw new ArgumentException($"Event has no texture file!");
            TextureSet effectTextures = Archive.ReadArchiveFromFile(effectTexturePath).ToTextureSet();

            EventPlayer result = new(cutscene, textures, effectTextures, null, uint.MaxValue, language, null, null, null, null);

            result.AudioController.VoiceVolume = voiceVolume;
            result.AudioController.ADXVolume = musicVolume;
            result.AudioController.SoundEffectVolume = sfxVolume;

            SetUpgrades(upgradeMask, result);

            return result;
        }

        private static WindowIcon GetIcon()
        {
            (int x, int y, int dim)[] icons = new (int x, int y, int dim)[]
            {
                (0, 0, 256),
                (256, 0, 128),
                (256, 128, 64),
                (320, 128, 48),
                (256, 192, 32),
                (368, 128, 16)
            };

            Image<Rgba32> image;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "SA3D.SA2CutscenePlayer.Properties.logo.png";
            using(Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new NullReferenceException())
            {
                image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
            }

            IconImage[] images = new IconImage[icons.Length];
            for(int i = 0; i < icons.Length; i++)
            {
                (int x, int y, int dim) = icons[i];
                Image<Rgba32> newImage = image.Clone(img => img.Crop(new(x, y, dim, dim)));
                byte[] byteData = new byte[dim * dim * 4];
                newImage.CopyPixelDataTo(byteData);
                images[i] = new(dim, dim, byteData);
            }

            return new(images);
        }
    }
}
