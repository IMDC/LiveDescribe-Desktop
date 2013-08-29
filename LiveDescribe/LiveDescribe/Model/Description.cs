using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Utilities;

namespace LiveDescribe.Model
{
    public class Description: INotifyPropertyChanged
    {
        private AudioUtility _audioutility;
        private string _filename;
        private bool _isextendeddescription;
        private double _startwavefiletime;
        private double _endwavefiletime;
        private double _actuallength;
        private double _startinvideo;
        private double _endinvideo;


        public Description(string filename, double startwavefiletime, double endwavefiletime, double startinvideo)
        {
            FileName = filename;
            StartWaveFileTime = startwavefiletime;
            EndWaveFileTime = endwavefiletime;
            StartInVideo = startinvideo;
        }

        #region Properties

        public AudioUtility AudioUtility
        {
            set
            {
                _audioutility = value;
                NotifyPropertyChanged("AudioUtility");
            }
            get
            {
                return _audioutility;
            }
        }

        public string FileName
        {
            set
            {
                _filename = value;
                NotifyPropertyChanged("FileName");
            }
            get
            {
                return _filename;
            }
        }

        public bool IsExtendedDescription
        {
            set
            {
                _isextendeddescription = value;
                NotifyPropertyChanged("IsExtendedDescription");
            }
            get
            {
                return _isextendeddescription;
            }
        }

        public double StartWaveFileTime
        {
            set
            {
                _startwavefiletime = value;
                NotifyPropertyChanged("StartWaveFileTime");
            }
            get
            {
                return _startwavefiletime;
            }
        }

        public double EndWaveFileTime
        {
            set
            {
                _endwavefiletime = value;
                NotifyPropertyChanged("EndWaveFileTime");
            }
            get
            {
                return _endwavefiletime;
            }
        }

        public double ActualLength
        {
            private set
            {
                _actuallength = value;
                NotifyPropertyChanged("ActualLength");
            }
            get
            {
                return _actuallength;
            }
        }

        public double StartInVideo
        {
            set
            {
                _startinvideo = value;
                NotifyPropertyChanged("StartInVideo");
            }
            get
            {
                return _startinvideo;
            }
        }

        public double EndInVideo
        {
            set
            {
                _endinvideo = value;
                NotifyPropertyChanged("EndInVideo");
            }
            get
            {
                return _endinvideo;
            }
        }
        #endregion

        #region PropertyChanged
        /// <summary>
        /// An event that notifies a subscriber that a property in this class has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property changed.</param>
        private void NotifyPropertyChanged(string propertyName)
        {
            /* Make a local copy of the event to prevent the case where the handler
             * will be set as null in-between the null check and the handler call.
             */
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
