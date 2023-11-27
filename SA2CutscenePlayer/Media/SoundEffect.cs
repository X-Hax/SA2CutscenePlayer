using SonicAudioLib.Archives;
using SonicAudioLib.CriMw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SA3D.SA2CutscenePlayer.Media
{
    public readonly struct SoundEffect
    {
        public readonly GCHandle? intro;
        public readonly GCHandle? loop;

        public SoundEffect(GCHandle? intro, GCHandle? loop)
        {
            this.intro = intro;
            this.loop = loop;
        }

        public static SoundEffect[] LoadCSB(string filepath)
        {
            // Based on CSB editor from SonicAudioTools by Skyth (blueskythlikesclouds)

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using CriTableReader reader = CriTableReader.Create(filepath);

            Dictionary<uint, (string name, byte flag)> cues = new();
            Dictionary<string, (byte[]? intro, byte[]? loop)> soundEffects = new();
            byte[] csbData = File.ReadAllBytes(filepath);

            while(reader.Read())
            {
                string headName = reader.GetString("name");
                if(headName == "SOUND_ELEMENT")
                {
                    long tablePosition = reader.GetPosition("utf");
                    using CriTableReader sdlReader = CriTableReader.Create(reader.GetSubStream("utf"));
                    while(sdlReader.Read())
                    {
                        if(sdlReader.GetByte("fmt") != 0)
                        {
                            throw new Exception("The given CSB file contains an audio file which is not an ADX. Only CSB files with ADXs are supported.");
                        }
                        else if(sdlReader.GetBoolean("stmflg"))
                        {
                            throw new Exception("csb requires external CPK");
                        }

                        string aaxName = Path.GetFileName(sdlReader.GetString("name"))[..^4];
                        long aaxPosition = sdlReader.GetPosition("data");
                        using Stream aaxSource = sdlReader.GetSubStream("data");
                        CriAaxArchive aaxArchive = new();
                        aaxArchive.Read(aaxSource);

                        byte[]? intro = null;
                        byte[]? loop = null;
                        foreach(CriAaxEntry? item in aaxArchive)
                        {
                            if(item == null)
                            {
                                continue;
                            }

                            byte[] adx = new byte[item.Length];
                            long position = tablePosition + aaxPosition + item.Position;
                            Array.Copy(csbData, position, adx, 0, adx.Length);

                            switch(item.Flag)
                            {
                                case CriAaxEntryFlag.Intro:
                                    if(intro != null)
                                    {
                                        throw new InvalidOperationException($"Two intros in \"{aaxName}\"");
                                    }

                                    intro = adx;
                                    break;
                                case CriAaxEntryFlag.Loop:
                                    if(loop != null)
                                    {
                                        throw new InvalidOperationException($"Two loops in \"{aaxName}\"");
                                    }

                                    loop = adx;
                                    break;
                            }
                        }

                        soundEffects.Add(aaxName, (intro, loop));
                    }
                }
                else if(headName == "CUE")
                {
                    using CriTableReader sdlReader = CriTableReader.Create(reader.GetSubStream("utf"));
                    while(sdlReader.Read())
                    {
                        string name = sdlReader.GetString("name");
                        uint id = sdlReader.GetUInt32("id");
                        byte flags = sdlReader.GetByte("flags");

                        cues.Add(id, (name, flags));
                    }
                }
            }

            // assemble the cues 
            SoundEffect[] result = new SoundEffect[cues.Keys.Max() + 1];
            foreach(KeyValuePair<uint, (string name, byte flag)> cue in cues)
            {
                string introName = cue.Value.name;
                string loopName = introName;
                if(introName.Contains('+'))
                {
                    int index = introName.LastIndexOf('+');
                    introName = introName[..index];
                    loopName = loopName[(index + 1)..];
                    loopName = introName[..^loopName.Length] + loopName;
                }

                byte[]? intro = soundEffects[introName].intro;
                byte[]? loop = cue.Value.flag != 0 ? soundEffects[loopName].loop ?? throw new InvalidDataException("No loop") : null;

                GCHandle? introHandle = intro == null ? null : GCHandle.Alloc(intro, GCHandleType.Pinned);
                GCHandle? loopHandle = loop == null ? null : GCHandle.Alloc(loop, GCHandleType.Pinned);

                result[cue.Key] = new(introHandle, loopHandle);
            }

            return result;
        }
    }
}
