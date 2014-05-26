using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LiveDescribe.Model
{
    public class Space : INotifyPropertyChanged
    {
        #region Instance Variables
        private double _startInVideo;
        private string _spaceText;
        private double _endInVideo;
        private double _length;
        private double _x;
        private double _y;
        private double _height;
        private double _width;
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
            StartInVideo = starttime;
            EndInVideo = endtime;
            DeleteSpaceCommand = new RelayCommand(DeleteSpace, () => true);

            SpaceMouseUpCommand = new RelayCommand(SpaceMouseUp, () => true);
            SpaceMouseDownCommand = new RelayCommand<MouseEventArgs>(SpaceMouseDown, param => true);
            SpaceMouseMoveCommand = new RelayCommand<MouseEventArgs>(SpaceMouseMove, param => true);
        }

        public Space() 
        {
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
                NotifyPropertyChanged("SpaceText");
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
                NotifyPropertyChanged("StartInVideo");
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
                NotifyPropertyChanged("EndInVideo");
            }
            get { return _endInVideo; }
        }

        [JsonIgnore]
        public double Length
        {
            set
            {
                _length = value;
                NotifyPropertyChanged("Length");
            }
            get { return _length; }
        }

        [JsonIgnore]
        public double X
        {
            set
            {
                _x = value;
                NotifyPropertyChanged("X");
            }
            get { return _x; }
        }

        [JsonIgnore]
        public double Y
        {
            set
            {
                _y = value;
                NotifyPropertyChanged("Y");
            }
            get { return _y; }
        }

        [JsonIgnore]
        public double Height
        {
            set
            {
                _height = value;
                NotifyPropertyChanged("Height");
            }
            get { return _height; }
        }

        [JsonIgnore]
        public double Width
        {
            set
            {
                _width = value;
                NotifyPropertyChanged("Width");
            }
            get { return _width; }
        }
        #endregion

        #region BindingFunctions

        /// <summary>
        /// Called when a delete space command is executed
        /// </summary>
        public void DeleteSpace()
        {
            Console.WriteLine("Space Deleted");
            EventHandler handler = SpaceDeleteEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is down on the space
        /// </summary>
        /// <param name="e"></param>
        public void SpaceMouseDown(MouseEventArgs e)
        {
            Console.WriteLine("Space Mouse Down");
            EventHandler handler = SpaceMouseDownEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when the mouse moves over the space
        /// </summary>
        /// <param name="e"></param>
        public void SpaceMouseMove(MouseEventArgs e)
        {
            Console.WriteLine("Space Mouse Move");
            EventHandler handler = SpaceMouseMoveEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when the mouse is up over a space
        /// </summary>
        public void SpaceMouseUp()
        {
            Console.WriteLine("Space Mouse Up");
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
