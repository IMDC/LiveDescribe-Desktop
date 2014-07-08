using System;
using System.ComponentModel;

namespace LiveDescribe.Interfaces
{
    public interface ILiveDescribePlayer : INotifyPropertyChanged
    {
        TimeSpan Position { get; set; }

        double DurationSeconds { get; }

        double DurationMilliseconds { get; }

        LiveDescribeVideoStates CurrentState { set; get; }

        string Path { set; get; }

        bool IsMuted { set; get; }

        double Volume { set; get; }

        void Play();

        void Pause();

        void Stop();

        void Close();
    }
}
