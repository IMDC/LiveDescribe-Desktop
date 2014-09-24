using LiveDescribe.Extensions;
using LiveDescribe.Model;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LiveDescribe.Utilities
{
    /// <summary>
    /// Samples .wav audio files in order to draw waveforms.
    /// </summary>
    public class AudioWaveFormSampler
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private Header _header;

        public AudioWaveFormSampler(string path)
        {
            Path = path;
        }

        public string Path { get; set; }

        public int SampleRatio { get; private set; }

        public Waveform CreateWaveform()
        {
            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                _header = ReadHeaderData(reader);
                //TODO: Find out why we want the ratio to be 80 when conditions are met.
                SampleRatio = _header.Channels == 2 ? 40 : 80;
                var sampleData = ReadSampleData(reader);
                return new Waveform(_header, new List<short>(sampleData), SampleRatio);
            }
        }

        private Header ReadHeaderData(BinaryReader r)
        {
            var header = new Header();

            var riffFileDescriptor = r.ReadChars(4);
            ValidateHeaderData(riffFileDescriptor, Header.RiffFileDescriptor);

            header.FileSize = r.ReadUInt32();
            var fileTypeHeader = r.ReadChars(4);
            ValidateHeaderData(fileTypeHeader, Header.FileTypeHeader);

            var formatChunkMarker = r.ReadChars(4);
            ValidateHeaderData(formatChunkMarker, Header.FormatChunkMarker);

            uint formatChunkSize = r.ReadUInt32();

            ushort audioFormat = r.ReadUInt16();
            if (audioFormat != Header.AudioFormat)
                throw new InvalidDataException("Audio format is incorrect: " + audioFormat);

            header.Channels = r.ReadUInt16();
            header.SampleRate = r.ReadUInt32();
            header.ByteRate = r.ReadUInt32();
            header.BlockAlign = r.ReadUInt16();
            header.BitsPerSample = r.ReadUInt16();

            /* There is a chance that the wave file will have a few extra bytes in its header. If
             * it does, then calculate how many and skip over them.
             */
            int extaFormatBytes = (Header.DefaultFormatChunkSize < formatChunkSize)
                ? (int)(formatChunkSize - Header.DefaultFormatChunkSize)
                : 0;

            r.ReadBytes(extaFormatBytes);

            var dataChunkMarker = r.ReadChars(4);
            ValidateHeaderData(dataChunkMarker, Header.DataChunkMarker);

            header.DataSize = r.ReadUInt32();

            if (r.BaseStream.Position != Header.DefaultHeaderByteSize + extaFormatBytes)
                throw new InvalidDataException("Stream is at the wrong position");

            return header;
        }

        private static void ValidateHeaderData(char[] data, char[] other)
        {
            if (!data.ElementsEquals(other))
                throw new InvalidDataException("Data is invalid: " + data);
        }

        private short[] ReadSampleData(BinaryReader r)
        {
            int bytesPerSample = _header.BitsPerSample / 8;

            var data = new short[_header.DataSize / bytesPerSample];

            for (int i = 0, index = 0; i < _header.DataSize; i += SampleRatio, index++)
            {
                data[index] = r.ReadInt16();
                r.ReadBytes(SampleRatio - 2);
            }

            return data;
        }
    }
}
