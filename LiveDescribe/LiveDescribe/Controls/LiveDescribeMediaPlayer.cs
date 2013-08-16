using System;
using System.Collections.Generic;
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

        public TimeSpan CurrentPosition
        {
            get
            {
                return this.Position; 
                
            }
        }

        public double DurationSeconds
        {
            get { return this.NaturalDuration.TimeSpan.TotalSeconds;}
            
        }

        public string Path
        {
            set
            {
                _path = value;
            }
            get
            {
                return _path;
            }
        }
    }
}
