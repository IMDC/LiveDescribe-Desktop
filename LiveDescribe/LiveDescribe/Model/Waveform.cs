using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
