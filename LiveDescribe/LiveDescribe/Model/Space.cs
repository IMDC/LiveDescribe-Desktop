using GalaSoft.MvvmLight.Command;
using LiveDescribe.Properties;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LiveDescribe.Model
{
    public class Space : DescribableInterval
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private bool _isRecordedOver;
        #endregion

        #region Event Handlers
        public event EventHandler SpaceDeleteEvent;
        public event EventHandler<MouseEventArgs> SpaceMouseUpEvent;
        public event EventHandler<MouseEventArgs> SpaceMouseDownEvent;
        public event EventHandler<MouseEventArgs> SpaceMouseMoveEvent;
        public event EventHandler GoToThisSpaceEvent;
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

            Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ColourScheme")
                    SetColour();
            };
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

        public override void SetColour()
        {
            if (IsSelected)
                Colour = Settings.Default.ColourScheme.SelectedItemColour;
            else if (IsRecordedOver)
                Colour = Settings.Default.ColourScheme.CompletedSpaceColour;
            else
                Colour = Settings.Default.ColourScheme.SpaceColour;
        }
        #endregion

        #region Property Changed
        protected override void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (propertyName == "IsSelected" || propertyName == "IsRecordedOver")
                SetColour();
        }
        #endregion
    }
}
