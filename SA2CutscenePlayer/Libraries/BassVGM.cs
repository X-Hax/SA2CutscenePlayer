using ManagedBass;
using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace SA3D.SA2CutscenePlayer.Libraries
{
    public static class BassVGM
    {
        [DllImport("bass_vgmstream", CharSet = CharSet.Ansi)]
        private static extern int BASS_VGMSTREAM_StreamCreate(string file, BassFlags flags);

        [DllImport("bass_vgmstream", CharSet = CharSet.Ansi)]
        private static extern int BASS_VGMSTREAM_StreamCreateFromMemory(nint buf, int bufsize, string name, BassFlags flags);

        public static int CreateStream(string file, BassFlags flags = BassFlags.Default)
        {
            int result = BASS_VGMSTREAM_StreamCreate(file, flags);
            if(result == 0)
            {
                throw new InvalidOperationException($"Unknown error occured trying to open file \"{file}\"!");
            }
            else if(result == -1)
            {
                throw new FileLoadException($"Error opening file \"{file}\"!");
            }
            else if(result == -2)
            {
                throw new FileLoadException($"Error creating BASS stream for file \"{file}\"!");
            }

            return result;
        }

        public static int CreateStream(GCHandle data, string name, BassFlags flags = BassFlags.Default)
        {
            if(data.Target is not byte[] target)
            {
                throw new InvalidOperationException("Data invalid");
            }

            int stream = BASS_VGMSTREAM_StreamCreateFromMemory(data.AddrOfPinnedObject(), target.Length, name, flags);

            if(stream == 0)
            {
                throw new InvalidOperationException($"Unknown error occured trying to read buffer \"{name}\"!");
            }
            else if(stream == -1)
            {
                throw new DataException($"Error read buffer \"{name}\"!");
            }
            else if(stream == -2)
            {
                throw new DataException($"Error creating BASS stream from buffer \"{name}\"!");
            }
            else if(stream == -3)
            {
                throw new DataException($"Invalid buffer for \"{name}\"!");
            }

            return stream;
        }
    }
}
