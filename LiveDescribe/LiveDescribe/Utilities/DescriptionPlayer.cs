using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;

namespace LiveDescribe.Utilities
{
    public class DescriptionPlayer : INotifyPropertyChanged
    {
        private bool _isPlaying;

        public bool IsPlaying
        {
            set
            {
                _isPlaying = value;
                NotifyPropertyChanged();
            }
            get { return _isPlaying; }
        }

        public bool CanPlay(Description description, double videoPositionMilliseconds)
        {
            double offset = videoPositionMilliseconds - description.StartInVideo;

            //if it is equal then the video time matches when the description should start dead on
            return
                !IsPlaying
                && ((!description.IsExtendedDescription
                        && 0 <= offset
                        && offset < description.WaveFileDuration)
                    || (description.IsExtendedDescription
                        && 0 <= offset
                        && offset < LiveDescribeConstants.ExtendedDescriptionStartIntervalMax));
        }

        public void Play(Description description, double videoPositionMilliseconds)
        {
            double offset = videoPositionMilliseconds - description.StartInVideo;

            description.Play(offset);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
