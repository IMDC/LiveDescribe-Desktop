using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Events;
using LiveDescribe.Model;
using NAudio.Wave;

namespace LiveDescribe.Utilities
{
    public class DescriptionPlayer : INotifyPropertyChanged
    {
        #region Fields
        private bool _isPlaying;
        private Description _playingDescription;
        private WaveOutEvent _descriptionStream;

        public event EventHandler<EventArgs<Description>> DescriptionFinishedPlaying;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public bool IsPlaying
        {
            set
            {
                _isPlaying = value;
                NotifyPropertyChanged();
            }
            get { return _isPlaying; }
        }

        #region Methods
        /// <summary>
        /// Determines if the given description can play at the given time.
        /// </summary>
        /// <param name="description">The description to check.</param>
        /// <param name="videoPositionMilliseconds">The time to check the description against.</param>
        /// <returns>Whether the description can be played or not.</returns>
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

        /// <summary>
        /// Plays the given description at the given time.
        /// </summary>
        /// <param name="description">Description to play.</param>
        /// <param name="videoPositionMilliseconds">Time to the description at.</param>
        public void Play(Description description, double videoPositionMilliseconds)
        {
            double offset = videoPositionMilliseconds - description.StartInVideo;

            var reader = new WaveFileReader(description.AudioFile);
            //reader.WaveFormat.AverageBytesPerSecond/ 1000 = Average Bytes Per Millisecond
            //AverageBytesPerMillisecond * (offset + StartWaveFileTime) = amount to play from
            reader.Seek((long)((reader.WaveFormat.AverageBytesPerSecond / 1000)
                * (offset + description.StartWaveFileTime)), SeekOrigin.Begin);
            var descriptionStream = new WaveOutEvent();
            descriptionStream.PlaybackStopped += DescriptionStream_PlaybackStopped;
            descriptionStream.Init(reader);

            _descriptionStream = descriptionStream;
            _playingDescription = description;

            descriptionStream.Play();

            IsPlaying = true;
        }

        /// <summary>
        /// Stops playback of the currently playing description, if there is one.
        /// </summary>
        public void Stop()
        {
            if (IsPlaying)
                IsPlaying = false;
            else
                return;

            if (_descriptionStream != null)
                _descriptionStream.Stop();
        }

        private void DescriptionStream_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            IsPlaying = false;
            OnDescriptionFinishedPlaying(_playingDescription);

            _descriptionStream.Dispose();
            _descriptionStream = null;
            _playingDescription = null;
        }
        #endregion

        #region Event Invokations
        private void OnDescriptionFinishedPlaying(Description description)
        {
            EventHandler<EventArgs<Description>> handler = DescriptionFinishedPlaying;
            if (handler != null) handler(this, new EventArgs<Description>(description));
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
