using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using NAudio;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class DescriptionRecordingControlViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private DescriptionRecorder _recorder;
        private ICommand _recordButtonClickCommand;
        private readonly ILiveDescribePlayer _mediaVideo;
        private readonly ProjectManager _projectManager;
        private bool _recordExtendedDescription;
        /// <summary>Used to restore the previous video state after it's finished recording.</summary>
        private LiveDescribeVideoStates _previousVideoState;

        #region Constructor
        public DescriptionRecordingControlViewModel(ILiveDescribePlayer mediaVideo, ProjectManager projectManager)
        {
            _mediaVideo = mediaVideo;
            _projectManager = projectManager;

            _recorder = GetDescriptionRecorder();

            InitCommands();
        }

        private void InitCommands()
        {
            RecordCommand = new RelayCommand(
                canExecute: () =>
                    Project != null
                    && _mediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded
                    && _recorder.CanRecord(),
                execute: () =>
                {
                    try
                    {
                        var pf = Project.GenerateDescriptionFile();
                        _recorder.RecordDescription(pf, RecordExtendedDescription, _mediaVideo.Position.TotalMilliseconds);
                        //save the current state so when the button is pressed again you can restore it back to that state
                        _previousVideoState = _mediaVideo.CurrentState;
                    }
                    catch (MmException e)
                    {
                        MessageBoxFactory.ShowError("No Microphone Connected");
                        Log.Warn("No Microphone Connected", e);
                    }
                    _mediaVideo.CurrentState = LiveDescribeVideoStates.RecordingDescription;
                    RecordButtonClickCommand = StopRecordingCommand;
                });

            StopRecordingCommand = new RelayCommand(
                canExecute: () =>
                    Project != null
                    && _mediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded
                    && _recorder.IsRecording,
                execute: () =>
                {
                    _recorder.StopRecording();
                    _mediaVideo.CurrentState = _previousVideoState;
                    RecordButtonClickCommand = RecordCommand;
                });
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for RecordCommand gets bound to the record button
        /// </summary>
        private ICommand RecordCommand { get; set; }
        private ICommand StopRecordingCommand { get; set; }

        public ICommand RecordButtonClickCommand
        {
            get { return _recordButtonClickCommand ?? (_recordButtonClickCommand = RecordCommand); }
            set
            {
                _recordButtonClickCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Properties

        public Project Project
        {
            get { return _projectManager.Project; }
        }

        public DescriptionRecorder Recorder
        {
            set
            {
                _recorder = value;
                RaisePropertyChanged();
            }
            get { return _recorder; }
        }

        /// <summary>
        /// Property that gets set when the extended description checkbox is checked or unchecked
        /// </summary>
        public bool RecordExtendedDescription
        {
            set
            {
                _recordExtendedDescription = value;
                RaisePropertyChanged();
            }
            get { return _recordExtendedDescription; }
        }
        #endregion

        #region Methods
        private DescriptionRecorder GetDescriptionRecorder()
        {
            var dr = new DescriptionRecorder();
            dr.DescriptionRecorded += (sender, args) =>
                _projectManager.AddDescriptionAndTrackForUndo(args.Value);
            return dr;
        }
        #endregion
    }
}
