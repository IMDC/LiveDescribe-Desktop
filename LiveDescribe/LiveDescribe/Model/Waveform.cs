using System.Collections.Generic;

namespace LiveDescribe.Model
{
    /// <summary>
    /// Represents a waveform sampled from a .wav file, along with its header.
    /// </summary>
    public class Waveform
    {
        private Waveform(Header header, List<short> d)
        {
            Header = header;
            Data = d;
        }

        public Waveform(Header header, List<short> data, int sampleRatio)
            : this(header, data)
        {
            SampleRatio = sampleRatio;
        }

        public Header Header { get; private set; }

        /// <summary>
        /// A Ratio of how many audio samples are taken for n bytes in the data array. For example,
        /// a SampleRatio of 40 means that for every 40 bytes in the sound file, 1 sample is saved
        /// from them.
        /// </summary>
        public int SampleRatio { get; private set; }

        public List<short> Data { get; private set; }
    }
}
