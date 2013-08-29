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

        /// <summary>
        /// Audio Utility that contains information about the wav description file
        /// </summary>
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
        /// <summary>
        /// Filename of the wav file
        /// </summary>
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

        /// <summary>
        /// Whether the description is extended or not
        /// </summary>
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

        /// <summary>
        /// The time that the description starts within the wav file
        /// for example it might start at 5 seconds instead of 0 (at the beginning)
        /// </summary>
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
        /// <summary>
        /// The time that the description ends within the wav file
        /// for example the description might end before the wav file actually finishes
        /// </summary>
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
        /// <summary>
        /// Actualy Length of the description
        /// </summary>
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
        /// <summary>
        /// The time in the video that the description starts
        /// </summary>
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
        /// <summary>
        /// The time in the video that the description ends
        /// </summary>
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
