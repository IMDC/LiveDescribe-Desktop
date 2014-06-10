using System.Collections.Generic;
using LiveDescribe.Utilities;

namespace LiveDescribe.Model
{
    /// <summary>
    /// Represents a waveform sampled from a .wav file, along with its header.
    /// </summary>
    public class Waveform
    {
        public Waveform(Header h, List<short> d)
        {
            Header = h;
            Data = d;
        }

        public Header Header { get; private set; }

        public List<short> Data { get; private set; }
    }
}
