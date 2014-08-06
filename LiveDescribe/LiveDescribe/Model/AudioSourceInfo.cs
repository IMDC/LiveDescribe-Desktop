using NAudio.Wave;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace LiveDescribe.Model
{
    public class AudioSourceInfo
    {
        public string Name { set; get; }
        public string Channels { set; get; }
        public int DeviceNumber { set; get; }
        public WaveInCapabilities Capabilities { set; get; }

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public AudioSourceInfo() { }

        public AudioSourceInfo(WaveInCapabilities capabilities, int deviceNumber)
        {
            Name = capabilities.ProductName;
            Channels = capabilities.Channels.ToString(CultureInfo.InvariantCulture);
            Capabilities = capabilities;
            DeviceNumber = deviceNumber;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("source", Capabilities, typeof(WaveInCapabilities));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var info = obj as AudioSourceInfo;
            if (info == null)
                return false;

            return (Name == info.Name)
                && (Channels == info.Channels)
                && (DeviceNumber == info.DeviceNumber);
        }

        public bool Equals(AudioSourceInfo info)
        {
            if (info == null)
                return false;

            return (Name == info.Name)
                && (Channels == info.Channels)
                && (DeviceNumber == info.DeviceNumber);
        }

        public override int GetHashCode()
        {
            String str = Name + Channels + DeviceNumber;
            return str.GetHashCode();
        }
    }
}
