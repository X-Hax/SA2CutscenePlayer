using ManagedBass;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using SA3D.SA2CutscenePlayer.Libraries;
using System;
using System.IO;
using System.Linq;

namespace SA3D.SA2CutscenePlayer.Media
{
    public class SoundEffectPlayer : IOutputDevice
    {

        private class ChannelInfo
        {
            private readonly int _index;
            private readonly SoundEffectPlayer _player;
            private bool _intro;

            public int Program { get; set; }
            public float Note { get; private set; }
            public float Volume { get; private set; }
            public float Pan { get; private set; }
            public float Pitch { get; private set; }

            private int _stream;
            private int _frequency;

            private bool HasStream
                => _stream != 0;

            private float PitchedFrequency
                // usually, you do -69 and /12, but this sounds the closest to the game, so...
                => (float)(Math.Pow(2, (Note + Pitch - 48) / 20f) * _frequency);

            public ChannelInfo(SoundEffectPlayer player, int index)
            {
                _player = player;
                _index = index;
                Volume = 1;
            }

            public void Reset()
            {
                Stop();
                Note = 0;
                Program = 0;
                Volume = 1;
                Pan = 0;
                Pitch = 0;
            }

            private void Play(bool start)
            {
                Stop();

                SoundEffect sound = _player._soundeffects[Program];
                if(start && sound.intro != null)
                {
                    _stream = BassVGM.CreateStream(sound.intro.Value, $"{_index}_{Program}_INTRO.adx");
                    _intro = true;
                }
                else if(sound.loop != null)
                {
                    _stream = BassVGM.CreateStream(sound.loop.Value, $"{_index}_{Program}_LOOP.adx", BassFlags.Loop);
                    _intro = false;
                }
                else
                {
                    _stream = 0;
                    _intro = false;
                }

                if(_stream != 0)
                {
                    Bass.ChannelGetInfo(_stream, out ManagedBass.ChannelInfo info);
                    _frequency = info.Frequency;

                    SetVolume(Volume);
                    SetPan(Pan);
                    SetPitch(Pitch);

                    Bass.ChannelPlay(_stream);
                }
            }

            public void SetVolume(float volume)
            {
                Volume = volume;
                if(HasStream)
                {
                    Bass.ChannelSetAttribute(_stream, ChannelAttribute.Volume, Volume * _player.Volume);
                }
            }

            public void SetPan(float pan)
            {
                Pan = pan;
                if(HasStream)
                {
                    Bass.ChannelSetAttribute(_stream, ChannelAttribute.Pan, Pan);
                }
            }

            public void SetPitch(float pitch)
            {
                Pitch = pitch;
                if(HasStream)
                {
                    Bass.ChannelSetAttribute(_stream, ChannelAttribute.Frequency, PitchedFrequency + pitch);
                }
            }

            public void Play(float note)
            {
                Note = note;
                Play(true);
            }

            public void Stop()
            {
                if(!HasStream)
                {
                    return;
                }

                if(_player._paused)
                {
                    Bass.ChannelPause(_stream);
                    return;
                }

                Bass.ChannelStop(_stream);
                Bass.StreamFree(_stream);

                _stream = 0;

                _intro = false;
            }

            public void Resume()
            {
                if(HasStream)
                {
                    Bass.ChannelPlay(_stream);
                }
            }

            public void Update()
            {
                if(!HasStream)
                {
                    return;
                }

                PlaybackState state = Bass.ChannelIsActive(_stream);
                if(_intro && state != PlaybackState.Playing)
                {
                    Play(false);
                }
            }
        }

        private float _volume;
        private readonly MidiFile _file;
        private readonly Playback _playback;
        private readonly SoundEffect[] _soundeffects;
        private readonly ChannelInfo[] _channels;
        private bool _paused;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                foreach(ChannelInfo channel in _channels)
                {
                    channel.SetVolume(value);
                }
            }
        }
        public SoundEffectPlayer(string mltDirectoryPath)
        {
            _volume = 1;
            _soundeffects = SoundEffect.LoadCSB(Path.Join(mltDirectoryPath, "4.csb"));

            _file = MidiFile.Read(
                Path.Join(mltDirectoryPath, Path.GetFileName(mltDirectoryPath) + "004000.mid"),
                new() { InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore });

            _playback = _file.GetPlayback(this);

            _channels = new ChannelInfo[(int)_file.GetChannels().Max() + 1];
            for(int i = 0; i < _channels.Length; i++)
            {
                _channels[i] = new(this, i);
            }
        }

        event EventHandler<MidiEventSentEventArgs> IOutputDevice.EventSent
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public void Reset()
        {
            foreach(ChannelInfo channel in _channels)
            {
                channel.Reset();
            }

            _playback.MoveToStart();
        }

        public void Start()
        {
            if(_paused)
            {
                foreach(ChannelInfo channel in _channels)
                {
                    channel.Resume();
                }

                _paused = false;
            }

            _playback.Start();
        }

        public void Pause()
        {
            _paused = true;
            _playback.Stop();
        }

        public void Update()
        {
            foreach(ChannelInfo channel in _channels)
            {
                channel.Update();
            }
        }

        void IDisposable.Dispose() { }

        void IOutputDevice.PrepareForEventsSending() { }

        void IOutputDevice.SendEvent(MidiEvent midiEvent)
        {
            switch(midiEvent.EventType)
            {
                case MidiEventType.ProgramChange:
                    ProgramChangeEvent programChange = (ProgramChangeEvent)midiEvent;
                    _channels[programChange.Channel].Program = programChange.ProgramNumber;
                    break;
                case MidiEventType.ControlChange:
                    ControlChangeEvent controlChange = (ControlChangeEvent)midiEvent;
                    ChannelInfo channel = _channels[controlChange.Channel];
                    ControlName controlChangeName = controlChange.GetControlName();

                    switch(controlChangeName)
                    {
                        case ControlName.ChannelVolume:
                            channel.SetVolume((byte)controlChange.ControlValue / (float)0x7F);
                            break;
                        case ControlName.ResetAllControllers:
                            channel.Reset();
                            break;
                        case ControlName.LsbForBankSelect:
                            break;
                        case ControlName.Pan:
                            int pan = (byte)controlChange.ControlValue - 64;
                            channel.SetPan(pan / (pan < 0 ? 64f : 63f));
                            break;
                        default:
                            Console.WriteLine($"Unimplemented Control change called: {controlChangeName}");
                            break;
                    }

                    break;
                case MidiEventType.NoteOn:
                    NoteOnEvent noteOn = (NoteOnEvent)midiEvent;
                    _channels[noteOn.Channel].Play(noteOn.NoteNumber);
                    break;
                case MidiEventType.NoteOff:
                    NoteOffEvent noteOff = (NoteOffEvent)midiEvent;
                    _channels[noteOff.Channel].Stop();
                    break;
                case MidiEventType.PitchBend:
                    PitchBendEvent pitchBend = (PitchBendEvent)midiEvent;

                    const float pitchBendValuesPerSemitone = 8192f / 4f;
                    float pitchBendValue = (pitchBend.PitchValue - 8192) / pitchBendValuesPerSemitone;

                    _channels[pitchBend.Channel].SetPitch(pitchBendValue);
                    break;
                case MidiEventType.TimeSignature:
                case MidiEventType.SetTempo:
                    break;
                default:
                    Console.WriteLine($"Unimplemented event called: {midiEvent.EventType}");
                    return;
            }
        }
    }
}
