using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using NAudio;

namespace LiveDescribe.ViewModel
{
    public class SpaceRecordingViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constants

        public const double CountdownTimerIntervalMsec = 1000 / 40; //40 times a second
        public const double DefaultMaxWordsPerMinute = 400;
        public const double DefaultMinWordsPerMinute = 0;
        #endregion

        #region Fields

        private bool _setDurationBasedOnWpm;
        private double _timeLeft;
        private double _elapsedTime;
        private double _initialTimeLeft;
        private Description _description;
        private Space _space;
        private readonly DescriptionRecorder _recorder;
        private readonly DescriptionPlayer _player;
        private readonly DispatcherTimer _recordingTimer;
        private readonly Stopwatch _stopwatch;
        /// <summary>Defines how long each word should be selected while recording a description.</summary>
        private double _timePerWordMsec;
        private double _wpmDuration;
        private double _wordsPerMinute;
        private double _maxWordsPerMinute;
        private double _minWordsPerMinute;
        private double _wordTimeAccumulator;
        /// <summary>Keeps track of Space Text words during recording.</summary>
        private PositionalStringTokenizer _tokenizer;
        #endregion

        #region Events
        public event EventHandler CloseRequested;
        public event EventHandler NextWordSelected;
        public event EventHandler RecordingEnded;
        public event EventHandler RecordingStarted;
        #endregion

        #region Constructor
        public SpaceRecordingViewModel(Space space, Project project)
        {
            InitCommands();

            _setDurationBasedOnWpm = false;
            _description = null;
            Space = space;
            Project = project;
            ResetElapsedTime();
            SetTimeLeft();
            MaxWordsPerMinute = DefaultMaxWordsPerMinute;
            MinWordsPerMinute = DefaultMinWordsPerMinute;

            _recorder = new DescriptionRecorder();
            _recorder.DescriptionRecorded += (sender, args) => Description = args.Value;

            _player = new DescriptionPlayer();
            _player.DescriptionFinishedPlaying += (sender, args) => CommandManager.InvalidateRequerySuggested();

            _recordingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(CountdownTimerIntervalMsec) };
            _recordingTimer.Tick += RecordingTimerOnTick;

            _stopwatch = new Stopwatch();

            CountdownControlViewModel = new CountdownControlViewModel();
            CountdownControlViewModel.CountdownFinished += (sender, args) => StartRecording();

            SetWpmValuesBasedOnSpaceText();
        }

        public void InitCommands()
        {
            RecordDescription = new RelayCommand(
                canExecute: () =>
                    Space != null
                    && _recorder.CanRecord()
                    && !_player.IsPlaying,
                execute: () =>
                {
                    if (_recorder.IsRecording)
                        StopRecording();
                    else if (CountdownControlViewModel.IsCountingDown)
                        CancelCountdown();
                    else
                        StartCountdown();
                });

            PlayRecordedDescription = new RelayCommand(
                canExecute: () =>
                    Description != null
                    && _player.CanPlay(_description)
                    && !_recorder.IsRecording
                    && !CountdownControlViewModel.IsCountingDown,
                execute: () => _player.Play(_description));

            SaveDescription = new RelayCommand(
                canExecute: () =>
                    Description != null
                    && !_recorder.IsRecording
                    && !CountdownControlViewModel.IsCountingDown,
                execute: () =>
                {
                    if (!string.IsNullOrWhiteSpace(_space.Text))
                        _description.Text = _space.Text;

                    _space.IsRecordedOver = true;
                    OnCloseRequested();
                });
        }
        #endregion

        #region Commands
        public ICommand RecordDescription { private set; get; }
        public ICommand PlayRecordedDescription { private set; get; }
        public ICommand SaveDescription { private set; get; }
        #endregion

        #region Properties
        /// <summary>
        /// Determines whether or not the viewmodel was successful in its job. If true, a
        /// description has been recorded and the user specified to add it to the project.
        /// </summary>
        public bool? DialogResult { set; get; }

        public PositionalStringTokenizer SpaceTextTokenizer { get { return _tokenizer; } }

        /// <summary>
        /// If set to true, this boolean will make the view model record for as much time as the
        /// wpm will allow for as opposed to the duration of the space.
        /// </summary>
        public bool SetDurationBasedOnWpm
        {
            set
            {
                _setDurationBasedOnWpm = value;
                RaisePropertyChanged();
            }
            get { return _setDurationBasedOnWpm; }
        }

        public double TimeLeft
        {
            set
            {
                _timeLeft = value;
                RaisePropertyChanged();
            }
            get { return _timeLeft; }
        }

        public double ElapsedTime
        {
            set
            {
                _elapsedTime = value;
                RaisePropertyChanged();
            }
            get { return _elapsedTime; }
        }

        public double WordsPerMinute
        {
            set
            {
                _wordsPerMinute = value;
                RaisePropertyChanged();
            }
            get { return _wordsPerMinute; }
        }

        public double MaxWordsPerMinute
        {
            set
            {
                _maxWordsPerMinute = value;
                RaisePropertyChanged();
            }
            get { return _maxWordsPerMinute; }
        }

        public double MinWordsPerMinute
        {
            set
            {
                _minWordsPerMinute = value;
                RaisePropertyChanged();
            }
            get { return _minWordsPerMinute; }
        }

        public double WpmDuration
        {
            set
            {
                _wpmDuration = value;
                RaisePropertyChanged();
            }
            get { return _wpmDuration; }
        }

        public Project Project { set; get; }

        public string Text
        {
            set
            {
                _space.Text = value;
                RaisePropertyChanged();
            }
            get { return _space.Text; }
        }

        public string TimeStamp { set; get; }

        public Description Description
        {
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
            get { return _description; }
        }

        public Space Space
        {
            set
            {
                _space = value;
                RaisePropertyChanged();
            }
            get { return _space; }
        }

        public DescriptionRecorder Recorder
        {
            get { return _recorder; }
        }

        public CountdownControlViewModel CountdownControlViewModel { private set; get; }
        #endregion

        #region Methods
        private void StartRecording()
        {
            try
            {
                var pf = Project.GenerateDescriptionFile();
                CalculateWordTime();
                _wordTimeAccumulator = 0;
                _initialTimeLeft = TimeLeft;
                _recorder.RecordDescription(pf, false, Space.StartInVideo);
                _recordingTimer.Start();
                _stopwatch.Start();
                OnRecordingStarted();
            }
            catch (MmException e)
            {
                MessageBoxFactory.ShowError("No Microphone Connected");
                Log.Warn("No Microphone Connected");
            }
        }

        private void SetWpmValuesBasedOnSpaceText()
        {
            TokenizeSpaceText();
            CalculateMinWordsPerMinute();
            WordsPerMinute = MinWordsPerMinute;
            CalculateWordTime();
        }

        private void TokenizeSpaceText()
        {
            _tokenizer = new PositionalStringTokenizer(Space.Text);
            _tokenizer.Tokenize();
        }

        private void CalculateMinWordsPerMinute()
        {
            if (string.IsNullOrWhiteSpace(Space.Text))
                MinWordsPerMinute = 0;
            else
                MinWordsPerMinute = Math.Min(MaxWordsPerMinute - 1,
                    (_tokenizer.Tokens.Count / (Space.Duration / Milliseconds.PerSecond)) * Seconds.PerMinute);
        }

        private void CalculateWordTime()
        {
            _timePerWordMsec = (!string.IsNullOrWhiteSpace(Space.Text))
                ? WpmDuration / _tokenizer.Tokens.Count
                : 0;
        }

        private void CalculateWpmDuration()
        {
            WpmDuration = (_tokenizer.Tokens.Count / WordsPerMinute) * Milliseconds.PerMinute;
        }

        private void StopRecording()
        {
            _recorder.StopRecording();
            _recordingTimer.Stop();
            _stopwatch.Reset();
            ResetElapsedTime();
            SetTimeLeft();
            OnRecordingEnded();
            CommandManager.InvalidateRequerySuggested();
        }

        private void ResetElapsedTime()
        {
            ElapsedTime = 0;
        }

        private void SetTimeLeft()
        {
            TimeLeft = (SetDurationBasedOnWpm)
                ? WpmDuration
                : Space.Duration;
        }

        private void StartCountdown()
        {
            CountdownControlViewModel.StartCountdown();
        }

        private void CancelCountdown()
        {
            CountdownControlViewModel.CancelCountdown();
        }

        protected override void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            base.RaisePropertyChanged(propertyName);

            switch (propertyName)
            {
                case "Text":
                    SetWpmValuesBasedOnSpaceText();
                    break;
                case "WordsPerMinute":
                    CalculateWpmDuration();
                    break;
                case "WpmDuration":
                    SetTimeLeft();
                    break;
                case "SetDurationBasedOnWpm":
                    SetTimeLeft();
                    break;
            }
        }

        private void RecordingTimerOnTick(object sender, EventArgs eventArgs)
        {
            ElapsedTime = _stopwatch.ElapsedMilliseconds;
            TimeLeft = _initialTimeLeft - ElapsedTime;

            if (_timePerWordMsec != 0 && _wordTimeAccumulator < ElapsedTime)
            {
                _wordTimeAccumulator += _timePerWordMsec;
                OnNextWordSelected();
            }

            if (TimeLeft <= 0 && _recorder.IsRecording)
                StopRecording();
        }

        #endregion

        #region Event Invokations

        private void OnCloseRequested()
        {
            EventHandler handler = CloseRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnNextWordSelected()
        {
            EventHandler handler = NextWordSelected;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnRecordingEnded()
        {
            EventHandler handler = RecordingEnded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnRecordingStarted()
        {
            EventHandler handler = RecordingStarted;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
