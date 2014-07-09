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

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TimerIntervalMilliseconds) };
            _timer.Tick += (sender, args) =>
            {
                CountdownTimeMsec -= TimerIntervalMilliseconds;
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
        }

        public void StopCountdown()
        {
            _timer.Stop();
            Visible = false;
            OnCountdownFinished();
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
