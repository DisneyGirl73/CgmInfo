using System;
using System.IO;

namespace CgmInfo
{
    internal static class StreamExtensions
    {
        public static ushort ReadWord(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "stream is null.");

            return (ushort)((stream.ReadByte() << 8) | stream.ReadByte());
        }
    }
}
