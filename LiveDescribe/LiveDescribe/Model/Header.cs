using System;

namespace LiveDescribe.Model
{
    /// <summary>
    /// Represents the header for a .wav file.
    /// </summary>
    [Serializable]
    public class Header
    {
        /// <summary>
        /// The default size of a wave header. Note that there could be extra parameters,
        /// increasing the size of the header.
        /// </summary>
        public const int DefaultHeaderByteSize = 44;
        public const uint DefaultFormatChunkSize = 16;
        public const ushort AudioFormat = 1;

        public static readonly char[] RiffFileDescriptor = { 'R', 'I', 'F', 'F' };
        public static readonly char[] FileTypeHeader = { 'W', 'A', 'V', 'E' };
        public static readonly char[] FormatChunkMarker = { 'f', 'm', 't', ' ' };
        public static readonly char[] DataChunkMarker = { 'd', 'a', 't', 'a' };

        public uint FileSize;
        public ushort Channels;
        public uint SampleRate;
        public uint ByteRate;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public uint DataSize;
    }
}
