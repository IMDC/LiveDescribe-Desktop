using GalaSoft.MvvmLight;
using System;
using System.Windows.Threading;

namespace LiveDescribe.ViewModel
{
    public class CountdownControlViewModel : ViewModelBase
    {
        #region Fields
        public const int CountdownStartingNumberMilliseconds = 3000;
        public const int TimerIntervalMilliseconds = 1000;

        private bool _visible;
        private bool _isCountingDown;
        private int _countdownTimeMsec;
        private readonly DispatcherTimer _timer;
        #endregion

        #region Events
        public event EventHandler CountdownFinished;
        #endregion

        #region Constructor
        public CountdownControlViewModel()
        {
            Visible = false;
            IsCountingDown = false;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TimerIntervalMilliseconds) };
            _timer.Tick += (sender, args) =>
            {
                CountdownTimeMsec -= TimerIntervalMilliseconds;

                //If there is no check to see if the time is equal to 0, then it will show the 0.
                if (CountdownTimeMsec < 0)
                    StopCountdown();
            };
        }
        #endregion

        #region Properties
        public bool Visible
        {
            set
            {
                _visible = value;
                RaisePropertyChanged();
            }
            get { return _visible; }
        }

        public bool IsCountingDown
        {
            private set
            {
                _isCountingDown = value;
                RaisePropertyChanged();
            }
            get { return _isCountingDown; }
        }

        public int CountdownTimeMsec
        {
            private set
            {
                _countdownTimeMsec = value;
                RaisePropertyChanged();
            }
            get { return _countdownTimeMsec; }
        }
        #endregion

        #region Methods
        public void StartCountdown()
        {
            CountdownTimeMsec = CountdownStartingNumberMilliseconds;
            Visible = true;
            _timer.Start();
            IsCountingDown = true;
        }

        public void StopCountdown()
        {
            CancelCountdown();
            OnCountdownFinished();
        }

        /// <summary>
        /// Stops the countdown timer without notifying observers that countdown has been completed.
        /// </summary>
        public void CancelCountdown()
        {
            _timer.Stop();
            Visible = false;
            IsCountingDown = false;
        }
        #endregion

        #region Event Invokations
        private void OnCountdownFinished()
        {
            EventHandler handler = CountdownFinished;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
