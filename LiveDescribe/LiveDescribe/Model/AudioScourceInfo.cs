using NAudio.Wave;
using System;
using System.Runtime.Serialization;

namespace LiveDescribe.Model
{
    public class AudioSourceInfo : ISerializable
    {
        public WaveInCapabilities Source { set; get; }
        public string Name { set; get; }
        public string Channels { set; get; }
        public int DeviceNumber { set; get; }
        public AudioSourceInfo(string name, string channels, WaveInCapabilities source, int deviceNumber)
        {
            Name = name;
            Channels = channels;
            Source = source;
            DeviceNumber = deviceNumber;
        }

        public AudioSourceInfo(SerializationInfo info, StreamingContext context)
        {
            Source = (WaveInCapabilities)info.GetValue("source", typeof(WaveInCapabilities));
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("source", Source, typeof(WaveInCapabilities));
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
