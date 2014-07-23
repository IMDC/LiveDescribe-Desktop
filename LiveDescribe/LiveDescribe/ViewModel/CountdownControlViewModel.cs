using System.Threading;
using GalaSoft.MvvmLight;
using System;
using System.Windows.Threading;
using LiveDescribe.Resources;
using NAudio.Wave;

namespace LiveDescribe.ViewModel
{
    public class CountdownControlViewModel : ViewModelBase
    {
        #region Fields
        public const int CountdownStartingNumberSeconds = 3;
        public const int TimerIntervalSeconds = 1;

        private bool _visible;
        private bool _isCountingDown;
        private int _countdownTimeSeconds;
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

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(TimerIntervalSeconds) };
            _timer.Tick += (sender, args) =>
            {
                CountdownTimeSeconds -= TimerIntervalSeconds;
                
                //If there is no check to see if the time is equal to 0, then it will show the 0.
                if (CountdownTimeSeconds == 0)
                    CustomResources.LongBeep.Play();
                else if (CountdownTimeSeconds < 0)
                    StopCountdown();
                else
                   CustomResources.Beep.Play();
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

        public int CountdownTimeSeconds
        {
            private set
            {
                _countdownTimeSeconds = value;
                RaisePropertyChanged();
            }
            get { return _countdownTimeSeconds; }
        }
        #endregion

        #region Methods
        public void StartCountdown()
        {
            CountdownTimeSeconds = CountdownStartingNumberSeconds;
            Visible = true;
            _timer.Start();
            CustomResources.Beep.Play();
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
