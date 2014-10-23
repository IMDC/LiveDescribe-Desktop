using System;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Properties;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LiveDescribe.Model
{
    public class Space : DescribableInterval, IComparable
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
        }

        public Space()
        {
            IsSelected = false;
            DeleteCommand = new RelayCommand(OnDeleteRequested, () => true);
            NavigateToCommand = new RelayCommand(OnNavigateToDescriptionRequested, () => true);

            MouseUpCommand = new RelayCommand<MouseEventArgs>(OnMouseUp, param => true);
            MouseDownCommand = new RelayCommand<MouseEventArgs>(OnMouseDown, param => true);
            MouseMoveCommand = new RelayCommand<MouseEventArgs>(OnMouseMove, param => true);
        }
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

            if (propertyName == "EndInVideo" || propertyName == "StartInVideo")
                UpdateDuration();
        }
        #endregion

        #region Compare Property
        public int CompareTo(object obj)
        {
            var tempSpace = (Space)obj;
            return this.StartInVideo.CompareTo(tempSpace.StartInVideo);
        }
        #endregion
    }
}
