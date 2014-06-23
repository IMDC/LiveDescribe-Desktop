using System.Diagnostics;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using System;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class SpaceRecordingViewModel : ViewModelBase
    {
        #region Constants

        public const double CountdownTimerIntervalMsec = 1000/40; //40 times a second
        #endregion

        #region Fields
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
        private double _wordTimeAccumulator;
        /// <summary>Keeps track of SpaceText words during recording.</summary>
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

            _description = null;
            Space = space;
            Project = project;
            ResetElapsedTime();
            ResetTimeLeft();

            _recorder = new DescriptionRecorder();
            _recorder.DescriptionRecorded += (sender, args) => Description = args.Value;

            _player = new DescriptionPlayer();
            _player.DescriptionFinishedPlaying += (sender, args) => CommandManager.InvalidateRequerySuggested();

            _recordingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(CountdownTimerIntervalMsec) };
            _recordingTimer.Tick += (sender, args) =>
            {
                ElapsedTime = _stopwatch.ElapsedMilliseconds;
                TimeLeft = Space.Duration - ElapsedTime;

                if (_timePerWordMsec != 0 && _wordTimeAccumulator < ElapsedTime)
                {
                    _wordTimeAccumulator += _timePerWordMsec;
                    OnNextWordSelected();
                }

                if (Space.Duration < ElapsedTime && _recorder.IsRecording)
                    StopRecording();
            };

            _stopwatch = new Stopwatch();
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
                    else
                        StartRecording();
                });

            PlayRecordedDescription = new RelayCommand(
                canExecute: () =>
                    Description != null
                    && _player.CanPlay(_description),
                execute: () => _player.Play(_description));

            SaveDescription = new RelayCommand(
                canExecute: () => Description != null,
                execute: () =>
                {
                    //Give Space text to description before exiting
                    _description.DescriptionText = _space.SpaceText;
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
        public PositionalStringTokenizer SpaceTextTokenizer { get { return _tokenizer; }}

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

        public Project Project { set; get; }

        public string Text
        {
            set
            {
                _space.SpaceText = value;
                RaisePropertyChanged();
            }
            get { return _space.SpaceText; }
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
        #endregion

        #region Methods
        private void StartRecording()
        {
            var pf = ProjectFile.FromAbsolutePath(Project.GenerateDescriptionFilePath(),
                Project.Folders.Descriptions);
            TokenizeSpaceText();
            CalculateWordTime();
            _wordTimeAccumulator = 0;
            _recorder.RecordDescription(pf, false, Space.StartInVideo);
            _recordingTimer.Start();
            _stopwatch.Start();
            OnRecordingStarted();
        }

        private void TokenizeSpaceText()
        {
            _tokenizer = new PositionalStringTokenizer(Space.SpaceText);
            _tokenizer.Tokenize();
        }

        private void CalculateWordTime()
        {
            _timePerWordMsec = (!string.IsNullOrWhiteSpace(Space.SpaceText))
                ? _timePerWordMsec = Space.Duration / _tokenizer.Tokens.Count
                : 0;
        }

        private void StopRecording()
        {
            _recorder.StopRecording();
            _recordingTimer.Stop();
            _stopwatch.Reset();
            ResetElapsedTime();
            ResetTimeLeft();
            OnRecordingEnded();
            CommandManager.InvalidateRequerySuggested();
        }

        private void ResetElapsedTime()
        {
            ElapsedTime = 0;
        }

        private void ResetTimeLeft()
        {
            TimeLeft = Space.Duration;
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
