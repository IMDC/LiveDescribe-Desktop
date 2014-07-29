using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Events;
using LiveDescribe.Extensions;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;
using LiveDescribe.View;
using Microsoft.Win32;
using NAudio;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        public readonly string DefaultWindowTitle = UiStrings.Program_Name;
        /// <summary>
        /// The span of time into a regular description that it can still be played.
        /// </summary>
        public const double PlayDescriptionThreshold = 0.8;
        #endregion

        #region Instance Variables
        private readonly ProjectManager _projectManager;
        private readonly Timer _descriptiontimer;
        private readonly MediaControlViewModel _mediaControlViewModel;
        private readonly PreferencesViewModel _preferences;
        private readonly LoadingViewModel _loadingViewModel;
        private readonly MarkingSpacesControlViewModel _markingSpacesControlViewModel;
        private readonly ILiveDescribePlayer _mediaVideo;
        private readonly DescriptionInfoTabViewModel _descriptionInfoTabViewModel;
        private readonly AudioCanvasViewModel _audioCanvasViewModel;
        private readonly DescriptionCanvasViewModel _descriptionCanvasViewModel;
        private readonly DescriptionRecordingControlViewModel _descriptionRecordingControlViewModel;
        private readonly UndoRedoManager _undoRedoManager;
        private Project _project;
        private string _windowTitle;
        private Description _lastRegularDescriptionPlayed;
        #endregion

        #region Events
        public event EventHandler GraphicsTick;
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler MediaEnded;
        public event EventHandler<EventArgs<Description>> PlayingDescription;
        #endregion

        #region Constructors
        public MainWindowViewModel(ILiveDescribePlayer mediaVideo)
        {
            DispatcherHelper.Initialize();

            _undoRedoManager = new UndoRedoManager();
            _loadingViewModel = new LoadingViewModel(100, null, 0, false);
            _projectManager = new ProjectManager(_loadingViewModel, _undoRedoManager);

            _mediaControlViewModel = new MediaControlViewModel(mediaVideo, _projectManager);
            _preferences = new PreferencesViewModel();
            _descriptionInfoTabViewModel = new DescriptionInfoTabViewModel(_projectManager);
            _markingSpacesControlViewModel = new MarkingSpacesControlViewModel(_descriptionInfoTabViewModel, mediaVideo);
            _audioCanvasViewModel = new AudioCanvasViewModel(mediaVideo, _projectManager, _undoRedoManager);
            _descriptionCanvasViewModel = new DescriptionCanvasViewModel(mediaVideo, _projectManager);
            _descriptionRecordingControlViewModel = new DescriptionRecordingControlViewModel(mediaVideo,
                _projectManager);

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
                canExecute: () => _projectManager.HasProjectLoaded,
                execute: () =>
                {
                    if (_projectManager.IsProjectModified)
                    {
                        var result = MessageBoxFactory.ShowWarningQuestion(
                            string.Format(UiStrings.MessageBox_Format_SaveProjectWarning, _project.ProjectName));

                        if (result == MessageBoxResult.Yes)
                            SaveProject.Execute();
                        else if (result == MessageBoxResult.Cancel)
                            return;
                    }

                    _projectManager.CloseProject();
                });


            NewProject = new RelayCommand(() =>
            {
                var viewModel = DialogShower.SpawnNewProjectView();

                if (viewModel.DialogResult != true)
                    return;

                if (viewModel.CopyVideo)
                    CopyVideoAndSetProject(viewModel.VideoPath, viewModel.Project);
                else
                    SetProject(viewModel.Project);
            });

            OpenProject = new RelayCommand(() =>
            {
                var projectChooser = new OpenFileDialog
                {
                    Filter = string.Format(UiStrings.OpenFileDialog_Format_OpenProject, Project.Names.ProjectExtension)
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
                    MessageBoxFactory.ShowError(UiStrings.MessageBox_OpenProjectFileMissingError);
                }
            });

            SaveProject = new RelayCommand(
                canExecute: () => _projectManager.IsProjectModified,
                execute: () => _projectManager.SaveProject()
            );

            ExportWithDescriptions = new RelayCommand(
                canExecute: () => _projectManager.HasProjectLoaded,
                execute: () =>
                {
                    var viewModel = DialogShower.SpawnExportWindowView(_project, _mediaVideo.Path,
                        _mediaVideo.DurationSeconds, _projectManager.RegularDescriptions.ToList(),
                        _loadingViewModel);

                    if (viewModel.DialogResult != true)
                        return;
                });

            ClearCache = new RelayCommand(
                canExecute: () => _projectManager.HasProjectLoaded,
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
                canExecute: () => _projectManager.HasProjectLoaded,
                execute: () =>
                {
                    var spaces = AudioAnalyzer.FindSpaces(_mediaControlViewModel.Waveform);
                    _projectManager.Spaces.AddRange(spaces);
                }
            );

            ExportDescriptionsTextToSrt = new RelayCommand(
                canExecute: () => _projectManager.HasProjectLoaded,
                execute: () =>
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = Path.GetFileNameWithoutExtension(_mediaControlViewModel.Path),
                        Filter = UiStrings.SaveFileDialog_ExportToSrt
                    };

                    saveFileDialog.ShowDialog();
                    FileWriter.WriteDescriptionsTextToSrtFile(saveFileDialog.FileName,
                        _projectManager.AllDescriptions);
                }
            );

            ExportSpacesTextToSrt = new RelayCommand(
                canExecute: () => _projectManager.HasProjectLoaded,
                execute: () =>
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = Path.GetFileNameWithoutExtension(_mediaControlViewModel.Path),
                        Filter = UiStrings.SaveFileDialog_ExportToSrt
                    };

                    saveFileDialog.ShowDialog();
                    FileWriter.WriteSpacesTextToSrtFile(saveFileDialog.FileName, _projectManager.Spaces);
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
                _descriptionRecordingControlViewModel.Recorder.MicrophoneDeviceNumber =
                    Settings.Default.Microphone.DeviceNumber;
                try
                {
                    Log.Info("Product Name of Apply Requested Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(
                        _descriptionRecordingControlViewModel.Recorder.MicrophoneDeviceNumber).ProductName);
                }
                catch (MmException)
                {
                    Log.Info("No Microphone is plugged in.");
                }
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

            _mediaControlViewModel.MuteRequested += OnMuteRequested;

            _mediaControlViewModel.MediaEndedEvent += (sender, e) =>
            {
                _descriptiontimer.Stop();
                _mediaVideo.Stop();
                OnMediaEnded(sender, e);
            };
            #endregion

            #region Property Changed Events

            _mediaControlViewModel.PropertyChanged += PropertyChangedHandler;

            //Update window title based on project name
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsProjectModified")
                    SetWindowTitle();
            };

            #endregion

            #region ProjectManager Events
            _projectManager.ProjectLoaded += (sender, args) =>
            {
                _project = args.Value;

                _mediaControlViewModel.LoadVideo(_project.Files.Video);

                SetWindowTitle();
            };

            _projectManager.ProjectModifiedStateChanged += (sender, args) => SetWindowTitle();

            _projectManager.ProjectClosed += (sender, args) =>
            {
                TryToCleanUpUnusedDescriptionAudioFiles();
                _project = null;

                SetWindowTitle();
            };
            #endregion

            SetWindowTitle();
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
        /// Command to export project Video along with the description track.
        /// </summary>
        public ICommand ExportWithDescriptions { private set; get; }

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

        /// <summary>
        /// Exports the text in all the descriptions to an SRT file
        /// </summary>
        public ICommand ExportDescriptionsTextToSrt { private set; get; }

        /// <summary>
        /// Exports the text in all the spaces to an srt file
        /// </summary>
        public ICommand ExportSpacesTextToSrt { private set; get; }
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

        public ProjectManager ProjectManager
        {
            get { return _projectManager; }
        }

        public MediaControlViewModel MediaControlViewModel
        {
            get { return _mediaControlViewModel; }
        }

        public PreferencesViewModel PreferencesViewModel
        {
            get { return _preferences; }
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

        public DescriptionRecordingControlViewModel DescriptionRecordingControlViewModel
        {
            get { return _descriptionRecordingControlViewModel; }
        }

        public UndoRedoManager UndoRedoManager
        {
            get { return _undoRedoManager; }
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
            foreach (var description in _projectManager.AllDescriptions)
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
                        OnPlayingDescription(description);
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
            MessageBoxFactory.ShowError(string.Format(UiStrings.MessageBox_Format_AudioFileNotFound, d.AudioFile));
        }

        /// <summary>
        /// Initializes and sets up the progam for a given project file.
        /// </summary>
        /// <param name="p">The project to initialize</param>
        public void SetProject(Project p)
        {
            CloseProject.ExecuteIfCan();

            _projectManager.LoadProject(p);
        }

        private void CopyVideoAndSetProject(string source, Project project)
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
                copier.CopyFile(source, project.Files.Video);
            };
            copyVideoWorker.ProgressChanged +=
                (sender, args) => LoadingViewModel.SetProgress("Copying Video File", args.ProgressPercentage);
            copyVideoWorker.RunWorkerCompleted += (sender, args) => SetProject(project);

            copyVideoWorker.RunWorkerAsync();
        }

        public bool TryExit()
        {
            if (_projectManager.IsProjectModified)
            {
                Log.Info("Program is attempting to exit with an unsaved project");

                var text = string.Format(UiStrings.MessageBox_Format_SaveProjectWarning, _project.ProjectName);

                var result = MessageBoxFactory.ShowWarningQuestion(text);

                if (result == MessageBoxResult.Yes)
                {
                    SaveProject.Execute();
                    TryToCleanUpUnusedDescriptionAudioFiles();
                    return true;
                }
                if (result == MessageBoxResult.No) //Exit but don't save
                {
                    Log.Info("User has chosen exit program and not save project");
                    TryToCleanUpUnusedDescriptionAudioFiles();
                    return true;
                }

                Log.Info("User has chosen not to exit program");
                return false;
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
                if (_descriptionRecordingControlViewModel.Recorder.IsRecording)
                    _descriptionRecordingControlViewModel.Recorder.StopRecording();

                _descriptiontimer.Stop();
                DescriptionPlayer.Dispose();
                FileDeleter.DeleteUnusedDescriptionFiles(_project);
            }
            catch (IOException e)
            {
                Log.Warn("File could not be deleted", e);
            }
        }

        private void SetWindowTitle()
        {
            if (_projectManager.IsProjectModified)
                WindowTitle = string.Format(UiStrings.Window_Format_MainWindowProjectModified,
                    _project.ProjectName, UiStrings.Program_Name);
            else if (_projectManager.HasProjectLoaded)
                WindowTitle = string.Format(UiStrings.Window_Format_MainWindowProjectSaved,
                    _project.ProjectName, UiStrings.Program_Name);
            else
                WindowTitle = DefaultWindowTitle;
        }
        #endregion

        #region Event Invokation Methods

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

        private void OnPlayingDescription(Description description)
        {
            var handler = PlayingDescription;
            if (handler != null) handler(this, description);
        }
        #endregion
    }
}
