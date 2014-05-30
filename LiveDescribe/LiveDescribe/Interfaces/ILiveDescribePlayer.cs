using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.Interfaces
{
    public interface ILiveDescribePlayer
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
