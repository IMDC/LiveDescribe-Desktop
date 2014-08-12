using GalaSoft.MvvmLight.Command;
using LiveDescribe.Properties;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LiveDescribe.Model
{
    public class Description : DescribableInterval
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        //All units of time is in milliseconds
        #region Instance variables
        private ProjectFile _audioFile;
        private bool _isextendeddescription;
        private bool _isPlaying;
        private double _startwavefiletime;
        private double _endwavefiletime;
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
            StartInVideo = startinvideo;

            if (!extendedDescription)
                EndInVideo = startinvideo + (endwavefiletime - startwavefiletime);
            else
                EndInVideo = startinvideo;
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
                    var pfi = new ProcessStartInfo("Explorer.exe", args);
                    Process.Start(pfi);
                });

            Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ColourScheme")
                    SetColour();
            };
        }

        #region Properties
        [JsonIgnore]
        public override double Duration
        {
            get { return EndInVideo - StartInVideo; }
            /* You should not be able to change the Duration of a description because it is based
             * on the length of the audio file. This might be changed later with description audio
             * trimming.
             */
            set { throw new NotImplementedException(); }
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
        /// The length of time the wave file is set to play for.
        /// </summary>
        [JsonIgnore]
        public double WaveFileDuration
        {
            get { return _endwavefiletime - _startwavefiletime; }
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

        public override void SetColour()
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

        #region Property Changed
        protected override void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (propertyName == "IsExtendedDescription" || propertyName == "IsSelected")
                SetColour();
        }
        #endregion
    }
}
