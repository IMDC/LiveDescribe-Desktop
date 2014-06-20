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

        public const double CountdownTimerIntervalMsec = 25; //40 times a second
        #endregion

        #region Fields

        private double _elapsedTime;
        private Description _description;
        private Space _space;
        private readonly DescriptionRecorder _recorder;
        private readonly DescriptionPlayer _player;
        private readonly DispatcherTimer _recordingTimer;
        private readonly Stopwatch _stopwatch;
        #endregion

        #region Events
        public event EventHandler CloseRequested;
        #endregion

        #region Constructor
        public SpaceRecordingViewModel(Space space, Project project)
        {
            InitCommands();

            _description = null;
            Space = space;
            Project = project;

            _recorder = new DescriptionRecorder();
            _recorder.DescriptionRecorded += (sender, args) => Description = args.Value;

            _player = new DescriptionPlayer();
            _player.DescriptionFinishedPlaying += (sender, args) => CommandManager.InvalidateRequerySuggested();

            _recordingTimer = new DispatcherTimer();
            _recordingTimer.Interval = TimeSpan.FromMilliseconds(CountdownTimerIntervalMsec);
            _recordingTimer.Tick += (sender, args) =>
            {
                ElapsedTime = _stopwatch.ElapsedMilliseconds;

                if (Space.Duration < ElapsedTime)
                {
                    RecordDescription.ExecuteIfCan();
                    _recordingTimer.Stop();
                    _stopwatch.Reset();
                    ElapsedTime = 0;
                    CommandManager.InvalidateRequerySuggested();
                }
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
                        _recorder.StopRecording();
                    else
                    {
                        var pf = ProjectFile.FromAbsolutePath(Project.GenerateDescriptionFilePath(),
                                Project.Folders.Descriptions);
                        _recorder.RecordDescription(pf, false, Space.StartInVideo);
                        _recordingTimer.Start();
                        _stopwatch.Start();
                    }
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

        #region Event Invokations

        public void OnCloseRequested()
        {
            EventHandler handler = CloseRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
