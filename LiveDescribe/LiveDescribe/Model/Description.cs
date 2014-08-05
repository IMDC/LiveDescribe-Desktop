using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Properties;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace LiveDescribe.Model
{
    public class Description : INotifyPropertyChanged, IDescribableInterval, IListIndexable
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        //All units of time is in milliseconds
        #region Instance variables
        private ProjectFile _audioFile;
        private string _text;
        private bool _isextendeddescription;
        private double _startwavefiletime;
        private double _endwavefiletime;
        private double _startinvideo;
        private double _endinvideo;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private bool _isSelected;
        private bool _isPlaying;
        private int _index;
        private Color _colour;
        #endregion

        #region Events
        public event EventHandler DescriptionDeleteEvent;
        public event EventHandler DescriptionMouseDownEvent;
        public event EventHandler DescriptionMouseUpEvent;
        public event EventHandler DescriptionMouseMoveEvent;
        public event EventHandler GoToThisDescriptionEvent;
        #endregion

        public Description(ProjectFile filepath, double startwavefiletime, double endwavefiletime,
            double startinvideo, bool extendedDescription)
            : this()
        {
            AudioFile = filepath;

            Text = Path.GetFileNameWithoutExtension(filepath);
            IsExtendedDescription = extendedDescription;

            //I specifically use the instance variables rather than the properties
            //because the property events can possibly be caught in the view
            //leading to an uneeded amount of changes to the description graphics
            _startwavefiletime = startwavefiletime;
            _endwavefiletime = endwavefiletime;
            _startinvideo = startinvideo;

            if (!extendedDescription)
                _endinvideo = startinvideo + (endwavefiletime - startwavefiletime);
            else
                _endinvideo = startinvideo;
        }

        public Description()
        {
            DescriptionMouseDownCommand = new RelayCommand<MouseEventArgs>(DescriptionMouseDown, param => true);
            GoToThisDescriptionCommand = new RelayCommand(GoTothisDescription, () => true);
            DescriptionMouseUpCommand = new RelayCommand(DescriptionMouseUp, () => true);
            DescriptionDeleteCommand = new RelayCommand(DescriptionDelete, () => true);
            DescriptionMouseMoveCommand = new RelayCommand<MouseEventArgs>(DescriptionMouseMove, param => true);

            OpenWinFileExplorerToFile = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    string args = string.Format("/Select, {0}", AudioFile);
                    ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
                    System.Diagnostics.Process.Start(pfi); 
                });
            
            Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ColourScheme")
                    SetColour();
            };
        }

        #region Properties
        /// <summary>
        /// Keeps track of the description's X values
        /// </summary>
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

        /// <summary>
        /// Keeps track of the description's Y value
        /// </summary>
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
        /// <summary>
        /// Keeps track of the height of the description
        /// </summary>
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

        /// <summary>
        /// Keeps track of the Width of the description
        /// </summary>
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

        /// <summary>
        /// Filename of the wav file
        /// </summary>
        public ProjectFile AudioFile
        {
            set
            {
                _audioFile = value;
                NotifyPropertyChanged();
            }
            get { return _audioFile; }
        }

        /// <summary>
        /// Whether the description is extended or not
        /// </summary>
        public bool IsExtendedDescription
        {
            set
            {
                _isextendeddescription = value;
                NotifyPropertyChanged();
            }
            get { return _isextendeddescription; }
        }

        /// <summary>
        /// The time that the description starts within the wav file for example it might start at 5
        /// seconds instead of 0 (at the beginning)
        /// </summary>
        public double StartWaveFileTime
        {
            set
            {
                _startwavefiletime = value;
                NotifyPropertyChanged();
            }
            get { return _startwavefiletime; }
        }
        /// <summary>
        /// The time that the description ends within the wav file for example the description might
        /// end before the wav file actually finishes
        /// </summary>
        public double EndWaveFileTime
        {
            set
            {
                _endwavefiletime = value;
                NotifyPropertyChanged();
            }
            get { return _endwavefiletime; }
        }

        /// <summary>
        /// The time in the video that the description starts
        /// </summary>
        public double StartInVideo
        {
            set
            {
                _startinvideo = value;
                NotifyPropertyChanged();
            }
            get { return _startinvideo; }
        }
        /// <summary>
        /// The time in the video that the description ends
        /// </summary>
        public double EndInVideo
        {
            set
            {
                _endinvideo = value;
                NotifyPropertyChanged();
            }
            get { return _endinvideo; }
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

        public string Text
        {
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
            get { return _text; }
        }

        public void SetStartAndEndInVideo(double startInVideo, double endInVideo)
        {
            _startinvideo = startInVideo;
            _endinvideo = endInVideo;
            NotifyPropertyChanged();
        }

        [JsonIgnore]
        public bool IsPlaying
        {
            set
            {
                _isPlaying = value;
                NotifyPropertyChanged();
            }
            get { return _isPlaying; }
        }

        /// <summary>
        /// The length of the span the description is set to play in the video.
        /// </summary>
        [JsonIgnore]
        public double Duration
        {
            get { return _endinvideo - _startinvideo; }
        }

        /// <summary>
        /// The length of time the wave file is set to play for.
        /// </summary>
        [JsonIgnore]
        public double WaveFileDuration
        {
            get { return _endwavefiletime - _startwavefiletime; }
        }

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

        [JsonIgnore]
        public Color Colour
        {
            set
            {
                _colour = value;
                NotifyPropertyChanged();
            }
            get { return _colour; }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for the DescriptionMouseDown Command
        /// </summary>
        [JsonIgnore]
        public RelayCommand<MouseEventArgs> DescriptionMouseDownCommand { get; private set; }

        /// <summary>
        /// Setter and getter for the DescriptionMouseUp Command
        /// </summary>
        [JsonIgnore]
        public RelayCommand DescriptionMouseUpCommand { get; private set; }

        /// <summary>
        /// Setter and getter for the DescriptionMouseMove Command
        /// </summary>
        [JsonIgnore]
        public RelayCommand<MouseEventArgs> DescriptionMouseMoveCommand { get; private set; }

        /// <summary>
        /// Setter and getter for DescriptionDeletecommand
        /// </summary>
        [JsonIgnore]
        public RelayCommand DescriptionDeleteCommand { get; private set; }

        [JsonIgnore]
        public RelayCommand GoToThisDescriptionCommand { get; private set; }

        [JsonIgnore]
        public ICommand OpenWinFileExplorerToFile { get; private set; }
        #endregion

        #region Methods

        private void SetColour()
        {
            if (IsSelected)
                Colour = Settings.Default.ColourScheme.SelectedItemColour;
            else if (IsExtendedDescription)
                Colour = Settings.Default.ColourScheme.ExtendedDescriptionColour;
            else
                Colour = Settings.Default.ColourScheme.RegularDescriptionColour;
        }
        #endregion

        #region Binding Functions

        /// <summary>
        /// Called when the mouse is up on this description
        /// </summary>
        private void DescriptionMouseUp()
        {
            EventHandler handler = DescriptionMouseUpEvent;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is down on this description
        /// </summary>
        /// <param name="e">the MouseEventArgs from the mouse down event</param>
        private void DescriptionMouseDown(MouseEventArgs e)
        {
            EventHandler handler = DescriptionMouseDownEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when the mouse is moving over a description
        /// </summary>
        /// <param name="e">the MouseEventArgs from the mouse move event</param>
        private void DescriptionMouseMove(MouseEventArgs e)
        {
            EventHandler handler = DescriptionMouseMoveEvent;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Called when a description is deleted
        /// </summary>
        private void DescriptionDelete()
        {
            Log.Info("Description deleted");
            EventHandler handler = DescriptionDeleteEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void GoTothisDescription()
        {
            EventHandler handler = GoToThisDescriptionEvent;
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
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }

            if (propertyName == "IsExtendedDescription" || propertyName == "IsSelected")
            {
                SetColour();
            }
        }
        #endregion
    }
}
