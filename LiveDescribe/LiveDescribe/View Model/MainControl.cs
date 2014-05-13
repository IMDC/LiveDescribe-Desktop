using System.IO;
using System.Web.Script.Serialization;
using LiveDescribe.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using System;
using LiveDescribe.Model;
using System.Timers;
using LiveDescribe.View;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LiveDescribe.View_Model
{
    class MainControl : ViewModelBase
    {
        #region Instance Variables
        private Timer _descriptiontimer;
        private VideoControl _videocontrol;
        private PreferencesViewModel _preferences;
        private DescriptionViewModel _descriptionviewmodel;
        private LoadingViewModel _loadingViewModel;
        private ILiveDescribePlayer _mediaVideo;
        private DescriptionInfoTabViewModel _descriptionInfoTabViewModel;
        private Project _project;
        #endregion

        #region Events
        public event EventHandler ProjectClosed;
        public event EventHandler GraphicsTick;
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler MediaEnded;
        #endregion

        #region Constructors
        public MainControl(ILiveDescribePlayer mediaVideo)
        {
            DispatcherHelper.Initialize();
            _loadingViewModel = new LoadingViewModel(100, null, 0, false);
            _videocontrol = new VideoControl(mediaVideo, _loadingViewModel);
            _preferences = new PreferencesViewModel();
            _descriptionviewmodel = new DescriptionViewModel(mediaVideo, _videocontrol);
            _descriptionInfoTabViewModel = new DescriptionInfoTabViewModel(_descriptionviewmodel);

            //Commands
            CloseProjectCommand = new RelayCommand(CloseProject, CanCloseProject);
            NewProjectCommand = new RelayCommand(NewProject);
            OpenProjectCommand = new RelayCommand(OpenProject);
            ShowPreferencesCommand = new RelayCommand(ShowPreferences);

            _mediaVideo = mediaVideo;

            //If apply requested happens  in the preferences use the new saved microphone in the settings
            _descriptiontimer = new Timer(10);
            _descriptiontimer.Elapsed += (sender, e) => Play_Tick(sender, e);
            _descriptiontimer.AutoReset = true;

            _preferences.ApplyRequested += (sender, e) =>
                {
                    _descriptionviewmodel.MicrophoneStream = Properties.Settings.Default.Microphone;
                    Console.WriteLine("Product Name of Apply Requested Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(_descriptionviewmodel.MicrophoneStream.DeviceNumber).ProductName);
                };

            _videocontrol.PlayRequested += (sender, e) =>
                {
                    _mediaVideo.Play();
                    _descriptiontimer.Start();
                    //this Handler should be attached to the view to update the graphics
                    EventHandler handler = this.PlayRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.PauseRequested += (sender, e) =>
                {
                    _mediaVideo.Pause();
                    _descriptiontimer.Stop();
                    //this Handler should be attached to the view to update the graphics
                    EventHandler handler = this.PauseRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.MuteRequested += (sender, e) =>
                {

                    //this Handler should be attached to the view to update the graphics
                    _mediaVideo.IsMuted = !_mediaVideo.IsMuted;
                    EventHandler handler = this.MuteRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.MediaEndedEvent += (sender, e) =>
                {
                    _descriptiontimer.Stop();
                    _mediaVideo.Stop();
                    EventHandler handler = this.MediaEnded;
                    if (handler != null) handler(sender, e);
                };
        }
        #endregion

        #region Commands
        public RelayCommand CloseProjectCommand { private set; get; }

        /// <summary>
        /// Command to open a new Project.
        /// </summary>
        public RelayCommand NewProjectCommand { private set; get; }

        /// <summary>
        /// Command to open an already existing project.
        /// </summary>
        public RelayCommand OpenProjectCommand { private set; get; }

        /// <summary>
        /// Command to show preferences
        /// </summary>
        public RelayCommand ShowPreferencesCommand { private set; get; }

        #endregion

        #region Command Functions

        public bool CanCloseProject()
        {
            //TODO: implement notifiable property?
            return _project != null;
        }

        /// <summary>
        /// This function gets called when the close project menu item gets pressed
        /// </summary>
        public void CloseProject()
        {
            //TODO: ask to save here before closing everything
            //TODO: put it in a background worker and create a loading screen (possibly a general use control)
            Console.WriteLine("Closed Project");

            _descriptionviewmodel.CloseDescriptionViewModel();
            _videocontrol.CloseVideoControl();
            _project = null;

            EventHandler handler = ProjectClosed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Opens a new project creation window, and on success sets up the new project.
        /// </summary>
        public void NewProject()
        {
            var viewModel = NewProjectViewModel.CreateWindow();

            if (viewModel.DialogResult != true)
                return;

            _project = viewModel.Project;


            //TODO: project created event?
            //Set up environment
            Properties.Settings.Default.WorkingDirectory = _project.ProjectFolderPath +"\\";
            _videocontrol.SetupAndStripAudio(_project.VideoFile.AbsolutePath);
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;
        }

        public void OpenProject()
        {
            var projectChooser = new OpenFileDialog
            {
                Filter = string.Format("LiveDescribe Files (*{0})|*{0}|All Files(*.*)|*.*",
                    Project.ProjectExtension)
            };

            bool? dialogSuccess = projectChooser.ShowDialog();
            if (dialogSuccess != true)
                return;

            var r = new StreamReader(projectChooser.FileName);
            Project p = JsonConvert.DeserializeObject<Project>(r.ReadToEnd());
            r.Close();

            _project = p;

            //TODO: project opened event?
            //Set up environment
            Properties.Settings.Default.WorkingDirectory = _project.ProjectFolderPath + "\\";
            _videocontrol.SetupAndStripAudio(_project.VideoFile.AbsolutePath);
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;
        }

        /// <summary>
        /// Gets called when the show preferences option is clicked
        /// </summary>
        public void ShowPreferences()
        {
            _preferences.InitializeAudioSourceInfo();
            var preferencesWindow = new PreferencesWindow(_preferences);
            preferencesWindow.ShowDialog();
        }
        #endregion

        #region Binding Properties
        /// <summary>
        /// returns the video control so it can be binded to a control in the mainwindow
        /// </summary>
        public VideoControl VideoControl
        {
            get { return _videocontrol; }
        }

        /// <summary>
        /// returns the PreferenceViewModel so it can be binded to a control in the main window
        /// </summary>
        public PreferencesViewModel PreferencesViewModel
        {
            get { return _preferences; }
        }

        /// <summary>
        /// returns the description view model so it can be binded to a control in the main window
        /// </summary>
        public DescriptionViewModel DescriptionViewModel
        {
            get { return _descriptionviewmodel; }
        }

        /// <summary>
        /// returns the loading view model so it can be binded to a control in the main window
        /// </summary>
        public LoadingViewModel LoadingViewModel
        {
            get { return _loadingViewModel; }
        }

        public DescriptionInfoTabViewModel DescriptionInfoTabViewModel
        {
            get { return _descriptionInfoTabViewModel; }
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Gets called by the description timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Tick(object sender, ElapsedEventArgs e)
        {

            EventHandler handler = GraphicsTick;
            if (handler != null) handler(sender, e);
            //I put this method in it's own timer in the MainControl for now, because I believe it should be separate from the view
            for (int i = 0; i < _descriptionviewmodel.AllDescriptions.Count; ++i)
            {
                Description currentDescription = _descriptionviewmodel.AllDescriptions[i];
                TimeSpan currentPositionInVideo = new TimeSpan();
                //get the current position of the video from the UI thread
                DispatcherHelper.UIDispatcher.Invoke(delegate { currentPositionInVideo = _mediaVideo.Position; });
                double offset = currentPositionInVideo.TotalMilliseconds - currentDescription.StartInVideo;

                if (!currentDescription.IsExtendedDescription &&
                    offset >= 0 && offset < (currentDescription.EndWaveFileTime - currentDescription.StartWaveFileTime))
                {
                    Console.WriteLine("Playing Regular Description");
                    currentDescription.Play(offset);
                    break;
                }
                else if (currentDescription.IsExtendedDescription &&
                    //if it is equal then the video time matches when the description should start dead on
                    offset < LiveDescribeConstants.EXTENDED_DESCRIPTION_START_INTERVAL_MAX && offset >= 0)
                {
                    DispatcherHelper.UIDispatcher.Invoke(delegate { _videocontrol.PauseCommand.Execute(this); Console.WriteLine("Playing Extended Description"); currentDescription.Play(); });
                    break;
                }
            }
        }
        #endregion
    }
}
