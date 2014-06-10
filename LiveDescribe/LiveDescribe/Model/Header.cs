using System;

namespace LiveDescribe.Utilities
{
    /// <summary>
    /// Represents the header for a .wav file.
    /// </summary>
    [Serializable]
    public class Header
    {
        public byte[] ChunkId;
        public uint ChunkSize;
        public byte[] Fmt;
        public byte[] SubChunk1Id;
        public uint SubChunk1Size;
        public ushort AudioFormat;
        public ushort NumChannels;
        public uint SampleRate;
        public uint ByteRate;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public byte[] SubChunk2Id;
        public uint SubChunk2Size;
    }
}
