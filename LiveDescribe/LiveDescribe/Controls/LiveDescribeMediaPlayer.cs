using LiveDescribe.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace LiveDescribe.Controls
{
    class LiveDescribeMediaPlayer : MediaElement, ILiveDescribePlayer
    {
        private string _path = "";
        private LiveDescribeVideoStates _currentState = LiveDescribeVideoStates.VideoNotLoaded;
        public EventHandler PathChangedEvent;

        /// <summary>
        /// Property for obtaining the CurrentState used to make the VideoPlayer Stateful, if it
        /// isn't already
        /// </summary>
        public LiveDescribeVideoStates CurrentState
        {
            get
            {
                return _currentState;
            }
            set
            {
                _currentState = value;
                OnPropertyChanged();
            }
        }

        public new bool IsMuted
        {
            get
            {
                return base.IsMuted;
            } 
            set 
            { 
                base.IsMuted = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Keep Track of the Duration in Seconds of the current video playing
        /// </summary>
        public double DurationSeconds
        {
            get { return NaturalDuration.TimeSpan.TotalSeconds; }
        }

        public double DurationMilliseconds
        {
            get { return NaturalDuration.TimeSpan.TotalMilliseconds; }
        }

        /// <summary>
        /// Keeps track of the current path of the file and throws an event if it is changed
        /// </summary>
        public string Path
        {
            set
            {
                _path = value;
                EventHandler handler = PathChangedEvent;
                if (handler == null) return;
                PathChangedEvent(this, EventArgs.Empty);
            }
            get
            {
                return _path;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
