using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Events;
using LiveDescribe.Extensions;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using LiveDescribe.View;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constants
        public const string DefaultWindowTitle = "Live Describe";
        /// <summary>
        /// The span of time into a regular description that it can still be played.
        /// </summary>
        public const double PlayDescriptionThreshold = 0.8;
        #endregion

        #region Instance Variables
        private readonly Timer _descriptiontimer;
        private readonly MediaControlViewModel _mediaControlViewModel;
        private readonly PreferencesViewModel _preferences;
        private readonly DescriptionCollectionViewModel _descriptioncollectionviewmodel;
        private readonly SpaceCollectionViewModel _spacecollectionviewmodel;
        private readonly LoadingViewModel _loadingViewModel;
        private readonly MarkingSpacesControlViewModel _markingSpacesControlViewModel;
        private readonly ILiveDescribePlayer _mediaVideo;
        private readonly DescriptionInfoTabViewModel _descriptionInfoTabViewModel;
        private readonly AudioCanvasViewModel _audioCanvasViewModel;
        private readonly DescriptionCanvasViewModel _descriptionCanvasViewModel;
        private Project _project;
        private string _windowTitle;
        private bool _projectModified;
        private Description _lastRegularDescriptionPlayed;
        #endregion

        #region Events
        public event EventHandler ProjectClosed;
        public event EventHandler GraphicsTick;
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler MediaEnded;
        public event EventHandler<EventArgs<Description>> OnPlayingDescription;
        #endregion

        #region Constructors
        public MainWindowViewModel(ILiveDescribePlayer mediaVideo)
        {
            DispatcherHelper.Initialize();
            WindowTitle = DefaultWindowTitle;

            _spacecollectionviewmodel = new SpaceCollectionViewModel(mediaVideo);
            _loadingViewModel = new LoadingViewModel(100, null, 0, false);
            _mediaControlViewModel = new MediaControlViewModel(mediaVideo, _loadingViewModel);
            _preferences = new PreferencesViewModel();
            _descriptioncollectionviewmodel = new DescriptionCollectionViewModel(mediaVideo, _mediaControlViewModel);
            _descriptionInfoTabViewModel = new DescriptionInfoTabViewModel(_descriptioncollectionviewmodel, _spacecollectionviewmodel);
            _markingSpacesControlViewModel = new MarkingSpacesControlViewModel(_descriptionInfoTabViewModel, mediaVideo);
            _audioCanvasViewModel = new AudioCanvasViewModel(_spacecollectionviewmodel, mediaVideo);
            _descriptionCanvasViewModel = new DescriptionCanvasViewModel(_descriptioncollectionviewmodel, mediaVideo);

            DescriptionPlayer = new DescriptionPlayer();
            DescriptionPlayer.DescriptionFinishedPlaying += (sender, e) =>
            {
                try
                {
                    DispatcherHelper.UIDispatcher.Invoke(() =>
                        _mediaControlViewModel.ResumeFromDescription(e.Value));
                }
                catch (TaskCanceledException exception)
                {
                    Log.Warn("Task Canceled Exception", exception);
                }
            };

            #region Commands
            //Commands
            CloseProject = new RelayCommand(
                canExecute: () => ProjectLoaded,
                execute: () =>
                {
                    if (ProjectModified)
                    {
                        var result = MessageBoxFactory.ShowWarningQuestion(
                            string.Format("The LiveDescribe project \"{0}\" has been modified." +
                            " Do you want to save changes before closing?", _project.ProjectName));

                        if (result == MessageBoxResult.Yes)
                            SaveProject.Execute();
                        else if (result == MessageBoxResult.Cancel)
                            return;
                    }

                    Log.Info("Closed Project");
                    TryToCleanUpUnusedDescriptionAudioFiles();
                    _descriptioncollectionviewmodel.CloseDescriptionCollectionViewModel();
                    _mediaControlViewModel.CloseMediaControlViewModel();
                    _spacecollectionviewmodel.CloseSpaceCollectionViewModel();
                    _project = null;
                    ProjectModified = false;

                    OnProjectClosed();

                    WindowTitle = DefaultWindowTitle;
                });


            NewProject = new RelayCommand(() =>
            {
                var viewModel = DialogShower.SpawnNewProjectView();

                if (viewModel.DialogResult != true)
                    return;

                if (viewModel.CopyVideo)
                {
                    LoadingViewModel.Visible = true;

                    //Copy video file in background while updating the LoadingBorder
                    var copyVideoWorker = new BackgroundWorker
                    {
                        WorkerReportsProgress = true,
                    };
                    var copier = new ProgressFileCopier();
                    copyVideoWorker.DoWork += (sender, args) =>
                    {
                        copier.ProgressChanged += (o, eventArgs) => copyVideoWorker.ReportProgress(eventArgs.ProgressPercentage);
                        copier.CopyFile(viewModel.VideoPath, viewModel.Project.Files.Video);
                    };
                    copyVideoWorker.ProgressChanged += (sender, args) => LoadingViewModel.SetProgress("Copying Video File", args.ProgressPercentage);
                    copyVideoWorker.RunWorkerCompleted += (sender, args) => SetProject(viewModel.Project);

                    copyVideoWorker.RunWorkerAsync();
                }
                else
                    SetProject(viewModel.Project);
            });

            OpenProject = new RelayCommand(() =>
            {
                var projectChooser = new OpenFileDialog
                {
                    Filter = string.Format("LiveDescribe Files (*{0})|*{0}|All Files(*.*)|*.*",
                        Project.Names.ProjectExtension)
                };

                bool? dialogSuccess = projectChooser.ShowDialog();
                if (dialogSuccess != true)
                    return;

                //Attempt to read project. If object fields are missing, an error window pops up.
                try
                {
                    Project p = FileReader.ReadProjectFile(projectChooser.FileName);
                    SetProject(p);
                }
                catch (JsonSerializationException)
                {
                    MessageBoxFactory.ShowError("The selected project is missing file locations.");
                }
            });

            SaveProject = new RelayCommand(
                canExecute: () => ProjectModified,
                execute: () =>
                {
                    FileWriter.WriteProjectFile(_project);

                    if (!Directory.Exists(_project.Folders.Cache))
                        Directory.CreateDirectory(_project.Folders.Cache);

                    FileWriter.WriteWaveFormHeader(_project, _mediaControlViewModel.Waveform.Header);
                    FileWriter.WriteWaveFormFile(_project, _mediaControlViewModel.Waveform.Data);
                    FileWriter.WriteDescriptionsFile(_project, _descriptioncollectionviewmodel.AllDescriptions);
                    FileWriter.WriteSpacesFile(_project, _spacecollectionviewmodel.Spaces);

                    ProjectModified = false;
                });

            ClearCache = new RelayCommand(
                canExecute: () => ProjectLoaded,
                execute: () =>
                {
                    var p = _project;

                    CloseProject.Execute();

                    Directory.Delete(p.Folders.Cache, true);

                    SetProject(p);
                });

            ShowPreferences = new RelayCommand(() =>
            {
                _preferences.InitializeAudioSourceInfo();
                var preferencesWindow = new PreferencesWindow(_preferences);
                preferencesWindow.ShowDialog();
            });

            FindSpaces = new RelayCommand(
                canExecute: () => ProjectLoaded,
                execute: () =>
                {
                    var spaces = AudioAnalyzer.FindSpaces(_mediaControlViewModel.Waveform);
                    _spacecollectionviewmodel.AddSpaces(spaces);
                }
            );

            ShowAboutInfo = new RelayCommand(DialogShower.SpawnAboutInfoView);
            #endregion

            _mediaVideo = mediaVideo;

            //If apply requested happens  in the preferences use the new saved microphone in the settings
            _descriptiontimer = new Timer(10);
            _descriptiontimer.Elapsed += Play_Tick;
            _descriptiontimer.AutoReset = true;

            _preferences.ApplyRequested += (sender, e) =>
            {
                _descriptioncollectionviewmodel.Recorder.MicrophoneDeviceNumber = Properties.Settings.Default.Microphone.DeviceNumber;
                Log.Info("Product Name of Apply Requested Microphone: " +
                    NAudio.Wave.WaveIn.GetCapabilities(_descriptioncollectionviewmodel.Recorder.MicrophoneDeviceNumber).ProductName);
            };

            #region MediaControlViewModel Events
            _mediaControlViewModel.PlayRequested += (sender, e) =>
            {
                _mediaVideo.Play();
                _descriptiontimer.Start();
                //this Handler should be attached to the view to update the graphics
                OnPlayRequested(sender, e);
            };

            _mediaControlViewModel.PauseRequested += (sender, e) =>
            {
                _mediaVideo.Pause();
                _descriptiontimer.Stop();
                if (_lastRegularDescriptionPlayed != null && _lastRegularDescriptionPlayed.IsPlaying)
                    DescriptionPlayer.Stop();
                //this Handler should be attached to the view to update the graphics
                OnPauseRequested(sender, e);
            };

            _mediaControlViewModel.OnPausedForExtendedDescription += (sender, e) =>
            {
                _mediaVideo.Pause();
                _descriptiontimer.Stop();
                CommandManager.InvalidateRequerySuggested();
            };

            _mediaControlViewModel.MuteRequested += (sender, e) =>
            {
                //this Handler should be attached to the view to update the graphics
                _mediaVideo.IsMuted = !_mediaVideo.IsMuted;
                OnMuteRequested(sender, e);
            };

            _mediaControlViewModel.MediaEndedEvent += (sender, e) =>
            {
                _descriptiontimer.Stop();
                _mediaVideo.Stop();
                OnMediaEnded(sender, e);
            };

            _mediaControlViewModel.OnStrippingAudioCompleted += (sender, args) =>
            {
                _spacecollectionviewmodel.AddSpaces(_mediaControlViewModel.Spaces);
                SaveProject.Execute();
            };
            #endregion

            #region Property Changed Events

            _spacecollectionviewmodel.Spaces.CollectionChanged += ObservableCollection_CollectionChanged;
            _descriptioncollectionviewmodel.ExtendedDescriptions.CollectionChanged += ObservableCollection_CollectionChanged;
            _descriptioncollectionviewmodel.RegularDescriptions.CollectionChanged += ObservableCollection_CollectionChanged;
            _mediaControlViewModel.PropertyChanged += PropertyChangedHandler;

            //Update window title based on project name
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ProjectModified")
                {
                    if (ProjectModified)
                        WindowTitle = string.Format("{0}* - LiveDescribe", _project.ProjectName);
                    else if (ProjectLoaded)
                        WindowTitle = string.Format("{0} - LiveDescribe", _project.ProjectName);
                    else
                        WindowTitle = DefaultWindowTitle;
                }
            };

            #endregion
        }
        #endregion

        #region Commands
        public ICommand CloseProject { private set; get; }

        /// <summary>
        /// Command to open a new Project.
        /// </summary>
        public ICommand NewProject { private set; get; }

        /// <summary>
        /// Command to open an already existing project.
        /// </summary>
        public ICommand OpenProject { private set; get; }

        /// <summary>
        /// Command to save project.
        /// </summary>
        public ICommand SaveProject { private set; get; }

        /// <summary>
        /// Command to clear the cache of the current project.
        /// </summary>
        public ICommand ClearCache { private set; get; }

        /// <summary>
        /// Command to show preferences
        /// </summary>
        public ICommand ShowPreferences { private set; get; }

        /// <summary>
        /// Finds spaces for the current project
        /// </summary>
        public ICommand FindSpaces { private set; get; }

        /// <summary>
        /// Opens the About window.
        /// </summary>
        public ICommand ShowAboutInfo { private set; get; }
        #endregion

        #region Properties

        public DescriptionPlayer DescriptionPlayer { private set; get; }

        /// <summary>
        /// The window title.
        /// </summary>
        public string WindowTitle
        {
            set
            {
                _windowTitle = value;
                RaisePropertyChanged();
            }
            get { return _windowTitle; }
        }

        public bool ProjectLoaded
        {
            get { return _project != null; }
        }

        /// <summary>
        /// Keeps track of whether the project has been modified or not by the program. This will be
        /// true iff there is a project loaded already.
        /// </summary>
        public bool ProjectModified
        {
            set
            {
                if (_projectModified != value)
                {
                    _projectModified = ProjectLoaded && value;
                    RaisePropertyChanged();
                }
            }
            get { return ProjectLoaded && _projectModified; }
        }

        public SpaceCollectionViewModel SpaceCollectionViewModel
        {
            get { return _spacecollectionviewmodel; }
        }

        public MediaControlViewModel MediaControlViewModel
        {
            get { return _mediaControlViewModel; }
        }

        public PreferencesViewModel PreferencesViewModel
        {
            get { return _preferences; }
        }

        public DescriptionCollectionViewModel DescriptionCollectionViewModel
        {
            get { return _descriptioncollectionviewmodel; }
        }
        public LoadingViewModel LoadingViewModel
        {
            get { return _loadingViewModel; }
        }

        public DescriptionInfoTabViewModel DescriptionInfoTabViewModel
        {
            get { return _descriptionInfoTabViewModel; }
        }

        public MarkingSpacesControlViewModel MarkingSpacesControlViewModel
        {
            get { return _markingSpacesControlViewModel; }
        }

        public AudioCanvasViewModel AudioCanvasViewModel
        {
            get { return _audioCanvasViewModel; }
        }

        public DescriptionCanvasViewModel DescriptionCanvasViewModel
        {
            get { return _descriptionCanvasViewModel; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Gets called by the description timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Tick(object sender, ElapsedEventArgs e)
        {
            OnGraphicsTick(sender, e);
            //I put this method in it's own timer in the MainWindowViewModel for now, because I believe it should be separate from the view
            foreach (var description in _descriptioncollectionviewmodel.AllDescriptions)
            {
                double videoPosition = 0;

                //get the current position of the video from the UI thread
                try
                {
                    DispatcherHelper.UIDispatcher.Invoke(() => { videoPosition = _mediaVideo.Position.TotalMilliseconds; });
                }
                catch (TaskCanceledException exception) { Log.Warn("Task Canceled Exception", exception); }

                try
                {
                    if (DescriptionPlayer.CanPlayInVideo(description, videoPosition))
                    {
                        PrepareForDescription(description);
                        DescriptionPlayer.PlayInVideo(description, videoPosition);
                        OnPlayingDescription(this, new EventArgs<Description>(description));
                    }
                }
                catch (Exception ex)
                {
                    if (ex is FileNotFoundException ||
                        ex is DirectoryNotFoundException)
                        DescriptionFileNotFound(description);
                    else if (ex is TaskCanceledException)
                        Log.Warn("Task Canceled Exception", ex);
                    else
                        throw;
                }
            }
        }

        private void PrepareForDescription(Description description)
        {
            if (description.IsExtendedDescription)
            {
                DispatcherHelper.UIDispatcher.Invoke(() =>
                {
                    // _mediaControlViewModel.PauseCommand.Execute(this);
                    _mediaControlViewModel.PauseForExtendedDescriptionCommand.Execute(this);
                    Log.Info("Playing Extended Description");
                });

                _descriptionInfoTabViewModel.SelectedExtendedDescription = description;
            }
            else
            {
                if (!description.IsPlaying)
                {
                    Log.Info("Playing Regular Description");
                    //Reduce volume on the graphics thread to avoid an invalid operation exception.
                    DispatcherHelper.UIDispatcher.Invoke(() => _mediaControlViewModel.ReduceVolume());
                }

                _lastRegularDescriptionPlayed = description;
                _descriptionInfoTabViewModel.SelectedRegularDescription = description;
            }
        }

        private void DescriptionFileNotFound(Description d)
        {
            //Pause from the UI thread.
            DispatcherHelper.UIDispatcher.Invoke(() => _mediaControlViewModel.PauseCommand.Execute());

            //TODO: Delete description if not found, or ask for file location?
            Log.ErrorFormat("The description file could not be found at {0}", d.AudioFile);
            MessageBoxFactory.ShowError("The audio file for description could not be found at " + d.AudioFile);
        }

        /// <summary>
        /// Initializes and sets up the progam for a given project file.
        /// </summary>
        /// <param name="p">The project to initialize</param>
        public void SetProject(Project p)
        {
            CloseProject.ExecuteIfCan();

            _project = p;

            WindowTitle = string.Format("{0} - LiveDescribe", _project.ProjectName);

            //Set up environment
            Properties.Settings.Default.WorkingDirectory = _project.Folders.Project + "\\";

            if (Directory.Exists(_project.Folders.Cache) && File.Exists(_project.Files.WaveForm))
            {
                var header = FileReader.ReadWaveFormHeader(_project);
                var audioData = FileReader.ReadWaveFormFile(_project);
                _mediaControlViewModel.Waveform = new Waveform(header, audioData);
                _mediaControlViewModel.Path = _project.Files.Video;
            }
            else
            {
                Directory.CreateDirectory(_project.Folders.Cache);

                _mediaControlViewModel.SetupAndStripAudio(_project);
            }

            if (Directory.Exists(_project.Folders.Descriptions))
            {
                if (File.Exists(_project.Files.Descriptions))
                {
                    var descriptions = FileReader.ReadDescriptionsFile(_project);
                    _descriptioncollectionviewmodel.AddDescriptions(descriptions);
                }
            }
            else
            {
                Directory.CreateDirectory(_project.Folders.Descriptions);
            }

            if (File.Exists(_project.Files.Spaces))
            {
                var spaces = FileReader.ReadSpacesFile(_project);
                _spacecollectionviewmodel.AddSpaces(spaces);
            }

            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;

            //Set Children
            _descriptioncollectionviewmodel.Project = _project;

            //Ensure that project is not modified.
            ProjectModified = false;
        }

        public bool TryExit()
        {
            if (ProjectModified)
            {
                Log.Info("Program is attempting to exit with an unsaved project");

                var text = string.Format("The LiveDescribe project \"{0}\" has been modified." +
                    " Do you want to save changes before closing?", _project.ProjectName);

                var result = MessageBox.Show(text, "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveProject.Execute();
                    TryToCleanUpUnusedDescriptionAudioFiles();
                    return true;
                }
                else if (result == MessageBoxResult.No) //Exit but don't save
                {
                    Log.Info("User has chosen exit program and not save project");
                    TryToCleanUpUnusedDescriptionAudioFiles();
                    return true;
                }
                else
                {
                    Log.Info("User has chosen not to exit program");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Attempt to delete all unused descriptions in the descriptions folder
        /// </summary>
        private void TryToCleanUpUnusedDescriptionAudioFiles()
        {
            try
            {
                if (_descriptioncollectionviewmodel.Recorder.IsRecording)
                    _descriptioncollectionviewmodel.Recorder.StopRecording();

                _descriptiontimer.Stop();
                DescriptionPlayer.Dispose();
                FileDeleter.DeleteUnusedDescriptionFiles(_project);
            }
            catch (IOException e)
            {
                Log.Warn("File could not be deleted", e);
            }
        }

        #endregion

        #region Event Handler Methods
        /// <summary>
        /// Adds a propertychanged handler to each new element of an observable collection, and
        /// removes one from each removed element.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Args</param>
        private void ObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var notifier = item as INotifyPropertyChanged;

                    if (notifier != null)
                        notifier.PropertyChanged += ObservableCollectionElement_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var notifier = item as INotifyPropertyChanged;

                    if (notifier != null)
                        notifier.PropertyChanged -= ObservableCollectionElement_PropertyChanged;
                }
            }

            ProjectModified = true;
        }

        /// <summary>
        /// Flags the current project as modified, so that the program (and user) know that it has
        /// been modified since the last save.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Args</param>
        private void ObservableCollectionElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO: Find a better way to implement this
            switch (e.PropertyName)
            {
                //Fallthrough cases
                case "AudioFile":
                case "IsExtendedDescription":
                case "StartWaveFileTime":
                case "EndWaveFileTime":
                case "ActualLength":
                case "StartInVideo":
                case "EndInVideo":
                case "Text":
                case "AudioData":
                case "Header":
                case "IsRecordedOver":
                    ProjectModified = true;
                    break;
            }
        }
        #endregion

        #region Event Invokation Methods

        private void OnProjectClosed()
        {
            EventHandler handler = ProjectClosed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnGraphicsTick(object sender, ElapsedEventArgs e)
        {
            EventHandler handler = GraphicsTick;
            if (handler != null) handler(sender, e);
        }

        private void OnPlayRequested(object sender, EventArgs e)
        {
            EventHandler handler = PlayRequested;
            if (handler != null) handler(sender, e);
        }

        private void OnPauseRequested(object sender, EventArgs e)
        {
            EventHandler handler = PauseRequested;
            if (handler != null) handler(sender, e);
        }

        private void OnMuteRequested(object sender, EventArgs e)
        {
            EventHandler handler = MuteRequested;
            if (handler != null) handler(sender, e);
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            EventHandler handler = MediaEnded;
            if (handler != null) handler(sender, e);
        }
        #endregion
    }
}
