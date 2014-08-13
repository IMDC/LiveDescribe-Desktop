using GalaSoft.MvvmLight.Command;
using LiveDescribe.Properties;
using Newtonsoft.Json;
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
            LockedInPlace = false;

            DeleteSpaceCommand = new RelayCommand(OnDeleteRequested, () => true);
            GoToThisSpaceCommand = new RelayCommand(OnNavigateToDescriptionRequested, () => true);

            SpaceMouseUpCommand = new RelayCommand<MouseEventArgs>(OnMouseUp, param => true);
            SpaceMouseDownCommand = new RelayCommand<MouseEventArgs>(OnMouseDown, param => true);
            SpaceMouseMoveCommand = new RelayCommand<MouseEventArgs>(OnMouseMove, param => true);

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
