﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Controls.UserControls;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;
using NAudio;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace LiveDescribe.Windows
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
        private bool _spaceHasText;
        private double _timeLeft;
        private double _elapsedTime;
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
        private double _recordDuration;
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
            RecordDuration = Space.Duration;
            MaxWordsPerMinute = DefaultMaxWordsPerMinute;
            MinWordsPerMinute = DefaultMinWordsPerMinute;

            _recorder = new DescriptionRecorder();
            _recorder.DescriptionRecorded += (sender, args) => Description = args.Value;

            _player = new DescriptionPlayer();
            _player.DescriptionFinishedPlaying += (sender, args) => CommandManager.InvalidateRequerySuggested();

            _recordingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(CountdownTimerIntervalMsec) };
            _recordingTimer.Tick += RecordingTimerOnTick;

            _stopwatch = new Stopwatch();

            CountdownViewModel = new CountdownViewModel();
            CountdownViewModel.CountdownFinished += (sender, args) => StartRecording();

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
                    else if (CountdownViewModel.IsCountingDown)
                        CancelCountdown();
                    else
                        StartCountdown();
                });

            PlayRecordedDescription = new RelayCommand(
                canExecute: () =>
                    Description != null
                        //&& _player.CanPlay(_description)
                    && !_recorder.IsRecording
                    && !CountdownViewModel.IsCountingDown,
                execute: () =>
                {
                    if (_player.IsPlaying)
                        _player.Stop();
                    else
                        _player.Play(_description);
                });

            SaveDescription = new RelayCommand(
                canExecute: () =>
                    Description != null
                    && !_recorder.IsRecording
                    && !CountdownViewModel.IsCountingDown,
                execute: () =>
                {
                    if (SpaceHasText)
                        _description.Text = _space.Text;

                    _description.Title = _space.Title;

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

        public bool SpaceHasText
        {
            set
            {
                _spaceHasText = value;
                RaisePropertyChanged();
            }
            get { return _spaceHasText; }
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

        public double RecordDuration
        {
            set
            {
                _recordDuration = value;
                RaisePropertyChanged();
            }
            get { return _recordDuration; }
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

        public DescriptionPlayer Player
        {
            get { return _player; }
        }

        public CountdownViewModel CountdownViewModel { private set; get; }
        #endregion

        #region Methods
        private void StartRecording()
        {
            try
            {
                var pf = Project.GenerateDescriptionFile();
                CalculateWordTime();
                _wordTimeAccumulator = 0;
                RecordDuration = TimeLeft;
                _recorder.RecordDescription(pf, false, Space.StartInVideo);
                _recordingTimer.Start();
                _stopwatch.Start();
                OnRecordingStarted();
            }
            catch (MmException e)
            {
                MessageBoxFactory.ShowError(UiStrings.MessageBox_NoMicrophoneFoundError);
                Log.Warn("No Microphone Connected", e);
            }
        }

        private void SetWpmValuesBasedOnSpaceText()
        {
            TokenizeSpaceText();
            CheckIfSpaceHasText();
            CalculateMinWordsPerMinute();
            WordsPerMinute = MinWordsPerMinute;
            CalculateWordTime();
        }

        private void CheckIfSpaceHasText()
        {
            SpaceHasText = !string.IsNullOrWhiteSpace(Space.Text);
        }

        private void TokenizeSpaceText()
        {
            _tokenizer = new PositionalStringTokenizer(Space.Text);
            _tokenizer.Tokenize();
        }

        private void CalculateMinWordsPerMinute()
        {
            if (SpaceHasText)
            {
                MinWordsPerMinute = Math.Min(MaxWordsPerMinute - 1,
                    (_tokenizer.Tokens.Count / (Space.Duration / Milliseconds.PerSecond)) * Seconds.PerMinute);
            }
            else
                MinWordsPerMinute = 0;
        }

        private void CalculateWordTime()
        {
            _timePerWordMsec = (SpaceHasText)
                ? WpmDuration / _tokenizer.Tokens.Count
                : 0;
        }

        private void CalculateWpmDuration()
        {
            WpmDuration = (SpaceHasText)
                ? (_tokenizer.Tokens.Count / WordsPerMinute) * Milliseconds.PerMinute
                : 0;
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
            TimeLeft = (SetDurationBasedOnWpm && SpaceHasText)
                ? WpmDuration
                : Space.Duration;
        }

        private void StartCountdown()
        {
            if (Recorder.MicrophoneAvailable())
                CountdownViewModel.StartCountdown();
            else
                MessageBoxFactory.ShowError(UiStrings.MessageBox_NoMicrophoneFoundError);
        }

        private void CancelCountdown()
        {
            CountdownViewModel.CancelCountdown();
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
            TimeLeft = RecordDuration - ElapsedTime;

            if (_timePerWordMsec != 0 && _wordTimeAccumulator < ElapsedTime)
            {
                _wordTimeAccumulator += _timePerWordMsec;
                OnNextWordSelected();
            }

            if (TimeLeft <= 0 && _recorder.IsRecording)
                StopRecording();
        }

        public void StopEverything()
        {
            if (CountdownViewModel.IsCountingDown)
                CountdownViewModel.CancelCountdown();

            if (Recorder.IsRecording)
                Recorder.StopRecording();

            if (Player.IsPlaying)
                Player.Stop();
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
