using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace LiveDescribe.Model
{
    public class Space : INotifyPropertyChanged
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private double _startInVideo;
        private string _spaceText;
        private double _endInVideo;
        private double _length;
        private double _x;
        private double _y;
        private double _height;
        private double _width;
        private bool _isSelected;
        #endregion

        #region Event Handlers
        [JsonIgnore]
        public EventHandler SpaceDeleteEvent;
        [JsonIgnore]
        public EventHandler SpaceMouseUpEvent;
        [JsonIgnore]
        public EventHandler SpaceMouseDownEvent;
        [JsonIgnore]
        public EventHandler SpaceMouseMoveEvent;
        #endregion

        #region Constructors
        public Space(double starttime, double endtime)
        {
            IsSelected = false;
            StartInVideo = starttime;
            EndInVideo = endtime;
            DeleteSpaceCommand = new RelayCommand(DeleteSpace, () => true);

            SpaceMouseUpCommand = new RelayCommand(SpaceMouseUp, () => true);
            SpaceMouseDownCommand = new RelayCommand<MouseEventArgs>(SpaceMouseDown, param => true);
            SpaceMouseMoveCommand = new RelayCommand<MouseEventArgs>(SpaceMouseMove, param => true);
        }

        public Space()
        {
            IsSelected = false;

            DeleteSpaceCommand = new RelayCommand(DeleteSpace, () => true);

            SpaceMouseUpCommand = new RelayCommand(SpaceMouseUp, () => true);
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
        public RelayCommand SpaceMouseUpCommand { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// Sets the text for the space
        /// </summary>
        public String SpaceText
        {
            set
            {
                _spaceText = value;
                NotifyPropertyChanged();
            }
            get
            {
                return _spaceText;
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
                NotifyPropertyChanged();
            }
            get { return _endInVideo; }
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

        #endregion

        #region BindingFunctions

        /// <summary>
        /// Called when a delete space command is executed
        /// </summary>
        public void DeleteSpace()
        {
            Log.Info("Space deleted");
            EventHandler handler = SpaceDeleteEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is down on the space
        /// </summary>
        /// <param name="e"></param>
        public void SpaceMouseDown(MouseEventArgs e)
        {
            EventHandler handler = SpaceMouseDownEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when the mouse moves over the space
        /// </summary>
        /// <param name="e"></param>
        public void SpaceMouseMove(MouseEventArgs e)
        {
            EventHandler handler = SpaceMouseMoveEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when the mouse is up over a space
        /// </summary>
        public void SpaceMouseUp()
        {
            EventHandler handler = SpaceMouseUpEvent;
            if (handler != null) handler(this, EventArgs.Empty);
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
