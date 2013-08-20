using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LiveDescribe.Interfaces;

namespace LiveDescribe.Controls
{
    class LiveDescribeMediaPlayer : MediaElement, ILiveDescribePlayer
    {
        private string _path = "";
        private LiveDescribeStates _currentState = LiveDescribeStates.VideoNotLoaded;
        public EventHandler PathChangedEvent;

        /// <summary>
        /// Property for obtaining the CurrentState used to make the VideoPlayer Stateful, if it isn't already
        /// </summary>
        public LiveDescribeStates CurrentState
        {
            get
            {
                return _currentState;
            }
            set
            {
                _currentState = value;
            }
        }

        /// <summary>
        /// Keep track of the CurrentPosition in the video the player is in
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get
            {
                return this.Position; 
            }
        }

        /// <summary>
        /// Keep Track of the Duration in Seconds of the current video playing
        /// </summary>
        public double DurationSeconds
        {
            get { return this.NaturalDuration.TimeSpan.TotalSeconds;}
            
        }

        public double DurationMilliseconds
        {
            get { return this.NaturalDuration.TimeSpan.TotalMilliseconds; }
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
    }
}
