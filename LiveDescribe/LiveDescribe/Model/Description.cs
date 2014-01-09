using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Utilities;
using LiveDescribe.Events;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework;
using Microsoft.TeamFoundation.MVVM;

namespace LiveDescribe.Model
{
    public class Description: INotifyPropertyChanged
    {
        //All units of time is in milliseconds
        #region Instance variables
        private AudioUtility _audioutility;
        private string _filename;
        private string _descriptiontext;
        private bool _isextendeddescription;
        private double _startwavefiletime;
        private double _endwavefiletime;
        private double _actuallength;
        private double _startinvideo;
        private double _endinvideo;
        private double _X;
        private double _Y;
        private double _width;
        private double _height;
        private bool _isselected;
        private bool _isPlaying;
        #endregion

        #region Event Handlers
        public EventHandler DescriptionMouseDownEvent;
        public EventHandler DescriptionMouseUpEvent;
        public EventHandler DescriptionMouseMoveEvent;
        public EventHandler DescriptionFinishedPlaying;
        #endregion

        public Description(string filename, double startwavefiletime, double endwavefiletime, double startinvideo, bool extendedDescription)
        {
            FileName = filename;
            DescriptionText = filename;
            _startwavefiletime = startwavefiletime;
            _endwavefiletime = endwavefiletime;
            _startinvideo = startinvideo;
            _endinvideo = startinvideo + (endwavefiletime - startwavefiletime);
            IsExtendedDescription = extendedDescription;
            DescriptionMouseDownCommand = new RelayCommand(DescriptionMouseDown, param => true);
            DescriptionMouseUpCommand = new RelayCommand(DescriptionMouseUp, param => true);
            DescriptionMouseMoveCommand = new RelayCommand(DescriptionMouseMove, param => true);
        }

        #region Public Methods
        /// <summary>
        /// This method plays the description from a specified offset in milliseconds
        /// </summary>
        ///<param name="offset">The offset in the file in which to play offset is in Milliseconds</param>
        /// <exception cref="FileNotFoundException">It is thrown if the path (filename) of the description does not exist</exception>
        public void Play(double offset)
        {
            if (IsPlaying == false)
                IsPlaying = true;
            else
            {
                //TODO: add a way to stop when the EndWaveFileTime has been reached
                //most likely using the reader variable, and the waveOut variable
                return;
            }
            NAudio.Wave.WaveFileReader reader = new NAudio.Wave.WaveFileReader(FileName);
            //reader.WaveFormat.AverageBytesPerSecond/ 1000 = Average Bytes Per Millisecond
            //AverageBytesPerMillisecond * (offset + StartWaveFileTime) = amount to play from
            reader.Seek((long)((reader.WaveFormat.AverageBytesPerSecond / 1000) * (offset + StartWaveFileTime)), System.IO.SeekOrigin.Begin);
            NAudio.Wave.WaveOutEvent waveOut = new NAudio.Wave.WaveOutEvent();
            waveOut.PlaybackStopped += OnDescriptionPlaybackStopped;
            waveOut.Init(reader);
            waveOut.Play();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// When the playback of the wave file stops naturally, this method gets called and sets
        /// the description IsPlaying value to false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDescriptionPlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            IsPlaying = false;
            EventHandler handler = DescriptionFinishedPlaying;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Keeps track of the description's X values
        /// </summary>
        public double X
        {
            set
            {
                _X = value;
                NotifyPropertyChanged("X");
            }
            get
            {
                return _X;
            }
        }

        /// <summary>
        /// Keeps track of the description's Y value
        /// </summary>
        public double Y
        {
            set
            {
                _Y = value;
                NotifyPropertyChanged("Y");
            }
            get
            {
                return _Y;
            }
        }
        /// <summary>
        /// Keeps track of the height of the description
        /// </summary>
        public double Height
        {
            set
            {
                _height = value;
                NotifyPropertyChanged("Height");
            }
            get
            {
                return _height;
            }
        }

        /// <summary>
        /// Keeps track of the Width of the description
        /// </summary>
        public double Width
        {
            set
            {
                _width = value;
                NotifyPropertyChanged("Width");
            }
            get
            {
                return _width;
            }
        }

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

        public bool IsSelected
        {
            set
            {
                _isselected = value;
                NotifyPropertyChanged("IsSelected");
            }
            get
            {
                return _isselected;
            }
        }

        public string DescriptionText
        {
            set
            {
                _descriptiontext = value;
                NotifyPropertyChanged("DescriptionText");
            }
            get
            {
                return _descriptiontext;
            }
        }

        public bool IsPlaying
        {
            set
            {
                _isPlaying = value;
                NotifyPropertyChanged("IsPlaying");
            }
            get
            {
                return _isPlaying;
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for the DescriptionMouseDown Command
        /// </summary>
        public RelayCommand DescriptionMouseDownCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and getter for the DescriptionMouseUp Command
        /// </summary>
        public RelayCommand DescriptionMouseUpCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and getter for the DescriptionMouseMove Command
        /// </summary>
        public RelayCommand DescriptionMouseMoveCommand
        {
            get;
            private set;
        }
        #endregion

        #region Binding Functions
        /// <summary>
        /// Called when the mouse is down on this description
        /// </summary>
        /// <param name="param"></param>
        public void DescriptionMouseDown(object param)
        {
            EventHandler handler = DescriptionMouseDownEvent;
            IsSelected = true;
            Console.WriteLine("Mouse Down");
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is up on this description
        /// </summary>
        /// <param name="param"></param>
        public void DescriptionMouseUp(object param)
        {
            EventHandler handler = DescriptionMouseUpEvent;
            IsSelected = false;
            Console.WriteLine("MOUSE UP");
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is moving on this description
        /// </summary>
        /// <param name="param"></param>
        public void DescriptionMouseMove(object param)
        {
            EventHandler handler = DescriptionMouseMoveEvent;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
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
