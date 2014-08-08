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

        /// <summary>
        /// The size of the entire wave file with the header included, in bytes.
        /// </summary>
        public uint FileSize;

        /// <summary>
        /// Number of Audio Channels in the wave file.
        /// </summary>
        public ushort Channels;

        /// <summary>
        /// Audio sample rate of the wave file.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// The number of bytes per second of data this file contains. Formula is
        /// (Sample Rate * BitsPerSample * Channels) / 8.
        /// </summary>
        public uint ByteRate;

        /// <summary>
        /// The number of bytes per sample for all channels. Formula is
        /// NumChannels * BitsPerSample/8.
        /// </summary>
        public ushort BlockAlign;

        /// <summary>
        /// The number of bits for a single channel sample. Usually 16.
        /// </summary>
        public ushort BitsPerSample;

        /// <summary>
        /// The size of the sound sample data. This does not include the header file.
        /// </summary>
        public uint DataSize;
    }
}
