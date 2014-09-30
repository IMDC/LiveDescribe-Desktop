﻿using GalaSoft.MvvmLight.Command;
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
        }

        public Space()
        {
            IsSelected = false;
            LockedInPlace = false;
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
        #endregion

        #region Property Changed
        protected override void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (propertyName == "EndInVideo" || propertyName == "StartInVideo")
                UpdateDuration();
        }
        #endregion
    }
}
