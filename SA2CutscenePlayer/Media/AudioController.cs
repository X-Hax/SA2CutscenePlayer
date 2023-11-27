using ManagedBass;
using SA3D.Archival.AFS;
using SA3D.SA2CutscenePlayer.Libraries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SA3D.SA2CutscenePlayer.Media
{
    public class AudioController
    {
        private bool _initialized;
        private float _voiceVolume;
        private float _adxVolume;

        private readonly SoundEffectPlayer? _soundEffectPlayer;

        public string? ADXDirectoryPath { get; set; }

        public float SoundEffectVolume
        {
            get => _soundEffectPlayer?.Volume ?? 0;
            set
            {
                if(_soundEffectPlayer != null)
                {
                    _soundEffectPlayer.Volume = value;
                }
            }
        }

        public float VoiceVolume
        {
            get => _voiceVolume;
            set
            {
                _voiceVolume = value;
                foreach(int stream in _activeVoiceStreams)
                {
                    Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, value);
                }
            }
        }

        public float ADXVolume
        {
            get => _adxVolume;
            set
            {
                _adxVolume = value;
                if(_activeADXStream != 0)
                {
                    Bass.ChannelSetAttribute(_activeADXStream, ChannelAttribute.Volume, value);
                }
            }
        }

        private int _activeADXStream;
        private readonly HashSet<int> _activeVoiceStreams = new();
        private readonly HashSet<int> _toRemove = new();

        private readonly GCHandle[] _voiceAudios;

        public AudioController(uint eventID, string? adxDirectoryPath, string? mltDirectoryPath, string? voiceArchiveFilepath, string? voiceArrayINIFilepath)
        {
            ADXDirectoryPath = adxDirectoryPath;
            VoiceVolume = 1;
            ADXVolume = 1;
            _soundEffectPlayer = mltDirectoryPath == null ? null : new(mltDirectoryPath);

            if(voiceArchiveFilepath != null)
            {
                AFSArchive archive = AFSArchive.ReadAFSArchiveFromFile(voiceArchiveFilepath);

                VoiceEntry[] allEntries = voiceArrayINIFilepath == null ? VoiceEntry.Default : VoiceEntry.LoadVoiceArrayINI(voiceArrayINIFilepath);
                IEnumerable<VoiceEntry> eventEntries = allEntries.Where(x => x.eventID == eventID);
                if(eventEntries.Any())
                {
                    _voiceAudios = new GCHandle[eventEntries.Max(x => x.voiceID) + 1];
                    foreach(VoiceEntry entry in eventEntries)
                    {
                        if(_voiceAudios[entry.voiceID] != default)
                        {
                            throw new InvalidDataException("Overlapping event voice indices!");
                        }

                        _voiceAudios[entry.voiceID] = GCHandle.Alloc(archive.AFSEntries[entry.fileID].Data.ToArray(), GCHandleType.Pinned);
                    }
                }
                else
                {
                    _voiceAudios = Array.Empty<GCHandle>();
                }
            }
            else
            {
                _voiceAudios = Array.Empty<GCHandle>();
            }

        }

        public void Init()
        {
            if(_initialized)
            {
                return;
            }

            _initialized = true;
            Bass.Init(Flags: DeviceInitFlags.Stereo);
        }

        public void PlayADX(string filename)
        {
            if(ADXDirectoryPath == null)
            {
                return;
            }

            if(_activeADXStream != 0)
            {
                StopADX();
            }

            string filepath = Path.Combine(ADXDirectoryPath, filename);
            if(!Path.Exists(filepath))
            {
                throw new FileNotFoundException("Could not find ADX file!", filepath);
            }

            _activeADXStream = BassVGM.CreateStream(filepath, BassFlags.SpeakerFront);
            Bass.ChannelSetAttribute(_activeADXStream, ChannelAttribute.Volume, ADXVolume);
            Bass.ChannelPlay(_activeADXStream);
        }

        public void StopADX()
        {
            Bass.ChannelStop(_activeADXStream);
            Bass.StreamFree(_activeADXStream);
            _activeADXStream = 0;
        }

        public void PlayVoice(int index)
        {
            if(_voiceAudios.Length <= index)
            {
                Console.WriteLine($"Tried playing voice {index}; Only {_voiceAudios.Length} voice audios available!");
                return;
            }

            int stream = BassVGM.CreateStream(_voiceAudios[index], index.ToString() + ".ahx", BassFlags.SpeakerFront);
            Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, VoiceVolume);
            Bass.ChannelPlay(stream);
            _activeVoiceStreams.Add(stream);
        }

        public void Reset()
        {
            StopADX();
            _soundEffectPlayer?.Reset();
            _soundEffectPlayer?.Start();
        }

        public void Update()
        {
            if(_activeADXStream != 0)
            {
                if(Bass.ChannelIsActive(_activeADXStream) != PlaybackState.Playing)
                {
                    StopADX();
                }
            }

            foreach(int stream in _activeVoiceStreams)
            {
                if(Bass.ChannelIsActive(stream) != PlaybackState.Playing)
                {
                    Bass.ChannelStop(stream);
                    Bass.StreamFree(stream);
                    _toRemove.Add(stream);
                }
            }

            _activeVoiceStreams.ExceptWith(_toRemove);
            _toRemove.Clear();

            _soundEffectPlayer?.Update();
        }

        public void Pause()
        {
            Bass.Pause();
            _soundEffectPlayer?.Pause();
        }

        public void Resume()
        {
            Bass.Start();
            _soundEffectPlayer?.Start();
        }
    }
}
