using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LiveDescribe.Model
{
    public class Space : INotifyPropertyChanged, IDescribableInterval, IListIndexable
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private double _startInVideo;
        private string _text;
        private double _endInVideo;
        private double _duration;
        private double _length;
        private double _x;
        private double _y;
        private double _height;
        private double _width;
        private bool _isSelected;
        private bool _isRecordedOver;
        private int _index;
        #endregion

        #region Event Handlers
        [JsonIgnore]
        public EventHandler SpaceDeleteEvent;
        [JsonIgnore]
        public EventHandler<MouseEventArgs> SpaceMouseUpEvent;
        [JsonIgnore]
        public EventHandler<MouseEventArgs> SpaceMouseDownEvent;
        [JsonIgnore]
        public EventHandler<MouseEventArgs> SpaceMouseMoveEvent;
        [JsonIgnore]
        public EventHandler GoToThisSpaceEvent;
        #endregion

        #region Constructors
        public Space(double starttime, double endtime)
            : this()
        {
            StartInVideo = starttime;
            EndInVideo = endtime;
            UpdateDuration();
        }

        public Space()
        {
            IsSelected = false;

            DeleteSpaceCommand = new RelayCommand(DeleteSpace, () => true);
            GoToThisSpaceCommand = new RelayCommand(GoToThisSpace, () => true);

            SpaceMouseUpCommand = new RelayCommand<MouseEventArgs>(SpaceMouseUp, param => true);
            SpaceMouseDownCommand = new RelayCommand<MouseEventArgs>(SpaceMouseDown, param => true);
            SpaceMouseMoveCommand = new RelayCommand<MouseEventArgs>(SpaceMouseMove, param => true);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and Getters for all Commands related to a Space
        /// </summary>
        [JsonIgnore]
        public RelayCommand DeleteSpaceCommand { get; private set; }
        [JsonIgnore]
        public RelayCommand<MouseEventArgs> SpaceMouseDownCommand { get; private set; }
        [JsonIgnore]
        public RelayCommand<MouseEventArgs> SpaceMouseMoveCommand { get; private set; }
        [JsonIgnore]
        public RelayCommand<MouseEventArgs> SpaceMouseUpCommand { get; private set; }
        [JsonIgnore]
        public RelayCommand GoToThisSpaceCommand { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// Sets the text for the space
        /// </summary>
        public String Text
        {
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// The start in video where the space starts
        /// </summary>
        public double StartInVideo
        {
            set
            {
                _startInVideo = value;
                UpdateDuration();
                NotifyPropertyChanged();
            }
            get { return _startInVideo; }
        }

        /// <summary>
        /// The the time in the video where the space ends
        /// </summary>
        public double EndInVideo
        {
            set
            {
                _endInVideo = value;
                UpdateDuration();
                NotifyPropertyChanged();
            }
            get { return _endInVideo; }
        }

        /// <summary>
        /// Duration of Space in Milliseconds
        /// </summary>
        [JsonIgnore]
        public double Duration
        {
            set
            {
                _duration = value;
                NotifyPropertyChanged();
            }
            get { return _duration; }
        }

        [JsonIgnore]
        public double Length
        {
            set
            {
                _length = value;
                NotifyPropertyChanged();
            }
            get { return _length; }
        }

        [JsonIgnore]
        public double X
        {
            set
            {
                _x = value;
                NotifyPropertyChanged();
            }
            get { return _x; }
        }

        [JsonIgnore]
        public double Y
        {
            set
            {
                _y = value;
                NotifyPropertyChanged();
            }
            get { return _y; }
        }

        [JsonIgnore]
        public double Height
        {
            set
            {
                _height = value;
                NotifyPropertyChanged();
            }
            get { return _height; }
        }

        [JsonIgnore]
        public double Width
        {
            set
            {
                _width = value;
                NotifyPropertyChanged();
            }
            get { return _width; }
        }

        [JsonIgnore]
        public bool IsSelected
        {
            set
            {
                _isSelected = value;
                NotifyPropertyChanged();
            }
            get { return _isSelected; }
        }

        /// <summary>
        /// Represents whether a description has been recorded in the duration of this space.
        /// </summary>
        public bool IsRecordedOver
        {
            set
            {
                _isRecordedOver = value;
                NotifyPropertyChanged();
            }
            get { return _isRecordedOver; }
        }

        /// <summary>
        /// The 1-based ndex of this space in a collection.
        /// </summary>
        [JsonIgnore]
        public int Index
        {
            set
            {
                _index = value;
                NotifyPropertyChanged();
            }
            get { return _index; }
        }
        #endregion

        #region Command Methods

        /// <summary>
        /// Called when a delete space command is executed
        /// </summary>
        private void DeleteSpace()
        {
            Log.Info("Space deleted");
            EventHandler handler = SpaceDeleteEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is down on the space
        /// </summary>
        /// <param name="e"></param>
        private void SpaceMouseDown(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = SpaceMouseDownEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when the mouse moves over the space
        /// </summary>
        /// <param name="e"></param>
        private void SpaceMouseMove(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = SpaceMouseMoveEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when the mouse is up over a space
        /// </summary>
        private void SpaceMouseUp(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = SpaceMouseUpEvent;
            if (handler != null) handler(this, e);
        }

        private void GoToThisSpace()
        {
            EventHandler handler = GoToThisSpaceEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion

        #region Methods

        private void UpdateDuration()
        {
            Duration = EndInVideo - StartInVideo;
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
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
        #endregion
    }
}
