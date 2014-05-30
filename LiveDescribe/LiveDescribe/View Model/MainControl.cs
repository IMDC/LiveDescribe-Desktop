﻿using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using LiveDescribe.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using System;
using LiveDescribe.Model;
using System.Timers;
using LiveDescribe.Utilities;
using LiveDescribe.View;
using Newtonsoft.Json;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Timer = System.Timers.Timer;

namespace LiveDescribe.View_Model
{
    class MainControl : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
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
        private Timer _descriptiontimer;
        private VideoControl _videocontrol;
        private PreferencesViewModel _preferences;
        private DescriptionViewModel _descriptionviewmodel;
        private SpacesViewModel _spacesviewmodel;
        private LoadingViewModel _loadingViewModel;
        private ILiveDescribePlayer _mediaVideo;
        private DescriptionInfoTabViewModel _descriptionInfoTabViewModel;
        private Project _project;
        private string _windowTitle;
        private bool _projectModified;
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
            WindowTitle = DefaultWindowTitle;
            _spacesviewmodel = new SpacesViewModel();
            _loadingViewModel = new LoadingViewModel(100, null, 0, false);
            _videocontrol = new VideoControl(mediaVideo, _loadingViewModel);
            _preferences = new PreferencesViewModel();
            _descriptionviewmodel = new DescriptionViewModel(mediaVideo, _videocontrol);
            _descriptionInfoTabViewModel = new DescriptionInfoTabViewModel(_descriptionviewmodel, _spacesviewmodel);

            #region Commands
            //Commands
            CloseProject = new RelayCommand(
                canExecute: () => _project != null,
                execute: () =>
                {
                    if (_projectModified)
                    {
                        var text = string.Format("The LiveDescribe project \"{0}\" has been modified." +
                            " Do you want to save changes before closing?", _project.ProjectName);
                        var result = MessageBox.Show(text, "Warning", MessageBoxButton.YesNoCancel,
                            MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                            SaveProject.Execute(null);
                        else if (result == MessageBoxResult.Cancel)
                            return;
                    }

                    log.Info("Closed Project");

                    _descriptionviewmodel.CloseDescriptionViewModel();
                    _videocontrol.CloseVideoControl();
                    _spacesviewmodel.CloseSpacesViewModel();
                    _project = null;

                    OnProjectClosed();

                    WindowTitle = DefaultWindowTitle;
                });


            NewProject = new RelayCommand(() =>
            {
                var viewModel = NewProjectViewModel.CreateWindow();

                if (viewModel.DialogResult != true)
                    return;

                if (viewModel.CopyVideo)
                {
                    LoadingViewModel.Visible = true;

                    //Copy video file in background while updating the LoadingBorder
                    var worker = new BackgroundWorker()
                    {
                        WorkerReportsProgress = true,
                    };
                    var copier = new ProgressFileCopier();
                    worker.DoWork += (sender, args) =>
                    {
                        copier.ProgressChanged += (o, eventArgs) => worker.ReportProgress(eventArgs.ProgressPercentage);
                        copier.CopyFile(viewModel.VideoPath, viewModel.Project.Files.Video);
                    };
                    worker.ProgressChanged += (sender, args) =>
                    {
                        LoadingViewModel.SetProgress("Copying Video File", args.ProgressPercentage);
                    };
                    worker.RunWorkerCompleted += (sender, args) => SetProject(viewModel.Project);

                    worker.RunWorkerAsync();
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
                    MessageBox.Show("The selected project is missing file locations.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            SaveProject = new RelayCommand(
                canExecute: () => _project != null && _projectModified,
                execute: () =>
                {
                    FileWriter.WriteProjectFile(_project);

                    if (!Directory.Exists(_project.Folders.Cache))
                        Directory.CreateDirectory(_project.Folders.Cache);

                    FileWriter.WriteWaveFormHeader(_project, _videocontrol.Header);
                    FileWriter.WriteWaveFormFile(_project, _videocontrol.AudioData);
                    FileWriter.WriteDescriptionsFile(_project, _descriptionviewmodel.AllDescriptions);
                    FileWriter.WriteSpacesFile(_project, _spacesviewmodel.Spaces);

                    ResetProjectModifiedFlag();
                });//(SaveProject, CanSaveProject);

            ClearCache = new RelayCommand(
                canExecute: () => _project != null,
                execute: () =>
                {
                    var p = _project;

                    CloseProject.Execute(null);

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
                canExecute: () => _project != null,
                execute: () =>
                {
                    var spaces = _videocontrol.FindSpaces(_project);
                    foreach (var space in spaces)
                    {
                        _spacesviewmodel.AddSpace(space);
                    }
                }
            );
            #endregion

            _mediaVideo = mediaVideo;

            //If apply requested happens  in the preferences use the new saved microphone in the settings
            _descriptiontimer = new Timer(10);
            _descriptiontimer.Elapsed += (sender, e) => Play_Tick(sender, e);
            _descriptiontimer.AutoReset = true;

            _preferences.ApplyRequested += (sender, e) =>
                {
                    _descriptionviewmodel.MicrophoneStream = Properties.Settings.Default.Microphone;
                    log.Info("Product Name of Apply Requested Microphone: " + 
                        NAudio.Wave.WaveIn.GetCapabilities(_descriptionviewmodel.MicrophoneStream.DeviceNumber).ProductName);
                };

            #region VideoControl Events
            _videocontrol.PlayRequested += (sender, e) =>
                {
                    _mediaVideo.Play();
                    _descriptiontimer.Start();
                    //this Handler should be attached to the view to update the graphics
                    OnPlayRequested(sender, e);
                };

            _videocontrol.PauseRequested += (sender, e) =>
                {
                    _mediaVideo.Pause();
                    _descriptiontimer.Stop();
                    //this Handler should be attached to the view to update the graphics
                    OnPauseRequested(sender, e);
                };

            _videocontrol.MuteRequested += (sender, e) =>
                {

                    //this Handler should be attached to the view to update the graphics
                    _mediaVideo.IsMuted = !_mediaVideo.IsMuted;
                    OnMuteRequested(sender, e);
                };

            _videocontrol.MediaEndedEvent += (sender, e) =>
                {
                    _descriptiontimer.Stop();
                    _mediaVideo.Stop();
                    OnMediaEnded(sender, e);
                };

            _videocontrol.OnStrippingAudioCompleted += (sender, args) =>
            {
                foreach (var space in _videocontrol.Spaces)
                {
                    _spacesviewmodel.AddSpace(space);
                }

                SaveProject.Execute(null);
            };
            #endregion

            #region Property Changed Events

            _spacesviewmodel.Spaces.CollectionChanged += ObservableCollection_CollectionChanged;
            _descriptionviewmodel.ExtendedDescriptions.CollectionChanged += ObservableCollection_CollectionChanged;
            _descriptionviewmodel.RegularDescriptions.CollectionChanged += ObservableCollection_CollectionChanged;
            _videocontrol.PropertyChanged += PropertyChangedHandler;

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
        #endregion

        #region Binding Properties

        /// <summary>
        /// The window title.
        /// </summary>
        public string WindowTitle
        {
            set
            {
                _windowTitle = value;
                RaisePropertyChanged("WindowTitle");
            }
            get { return _windowTitle; }
        }


        /// <summary>
        /// returns the spaces view model so a control in the main window can use it as a data context
        /// </summary>
        public SpacesViewModel SpacesViewModel
        {
            get { return _spacesviewmodel; }
        }

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
            OnGraphicsTick(sender, e);
            //I put this method in it's own timer in the MainControl for now, because I believe it should be separate from the view
            for (int i = 0; i < _descriptionviewmodel.AllDescriptions.Count; i++)
            //foreach (var description in _descriptionviewmodel.AllDescriptions)
            {
                var description = _descriptionviewmodel.AllDescriptions[i];
                TimeSpan currentPositionInVideo = new TimeSpan();
                //get the current position of the video from the UI thread
                DispatcherHelper.UIDispatcher.Invoke(() => { currentPositionInVideo = _mediaVideo.Position; });
                double offset = currentPositionInVideo.TotalMilliseconds - description.StartInVideo;

                if (!description.IsExtendedDescription &&
                    //
                    0 <= offset && offset < description.WaveFileDuration * PlayDescriptionThreshold)
                {
                    if (!description.IsPlaying)
                    {
                        log.Info("Playing Regular Description");
                        //Reduce volume on the graphics thread to avoid an invalid operation exception.
                        DispatcherHelper.UIDispatcher.Invoke(() => _videocontrol.ReduceVolume());
                    }

                    description.Play(offset);
                    break;
                }
                else if (description.IsExtendedDescription &&
                    //if it is equal then the video time matches when the description should start dead on
                    0 <= offset && offset < LiveDescribeConstants.ExtendedDescriptionStartIntervalMax)
                {
                    log.Info("Playing Extended Description");

                    DispatcherHelper.UIDispatcher.Invoke(() =>
                    {
                        _videocontrol.PauseCommand.Execute(this);
                        log.Info("Playing Extended Description");
                        description.Play();
                    });
                    break;
                }
            }
        }

        /// <summary>
        /// Initializes and sets up the progam for a given project file.
        /// </summary>
        /// <param name="p">The project to initialize</param>
        public void SetProject(Project p)
        {
            if (CloseProject.CanExecute(null))
                CloseProject.Execute(null);

            _project = p;

            WindowTitle = string.Format("{0} - LiveDescribe", _project.ProjectName);

            //Set up environment
            Properties.Settings.Default.WorkingDirectory = _project.Folders.Project + "\\";

            if (Directory.Exists(_project.Folders.Cache) && File.Exists(_project.Files.WaveForm))
            {
                _videocontrol.Header = FileReader.ReadWaveFormHeader(_project);
                _videocontrol.AudioData = FileReader.ReadWaveFormFile(_project);
                _videocontrol.Path = _project.Files.Video;
            }
            else
            {
                Directory.CreateDirectory(_project.Folders.Cache);

                _videocontrol.SetupAndStripAudio(_project);
            }

            if (Directory.Exists(_project.Folders.Descriptions))
            {
                if (File.Exists(_project.Files.Descriptions))
                {
                    var descriptions = FileReader.ReadDescriptionsFile(_project);

                    foreach (Description d in descriptions)
                    {
                        _descriptionviewmodel.AddDescription(d);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(_project.Folders.Descriptions);
            }

            if (File.Exists(_project.Files.Spaces))
            {
                var spaces = FileReader.ReadSpacesFile(_project);
                foreach (var s in spaces)
                {
                    _spacesviewmodel.AddSpace(s);
                }
            }

            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;

            //Set Children
            _descriptionviewmodel.Project = _project;

            //Ensure that project is not modified.
            ResetProjectModifiedFlag();
        }

        private void FlagProjectAsModified()
        {
            _projectModified = true;
            WindowTitle = string.Format("{0}* - LiveDescribe", _project.ProjectName);
        }

        private void ResetProjectModifiedFlag()
        {
            _projectModified = false;
            WindowTitle = string.Format("{0} - LiveDescribe", _project.ProjectName);
        }

        public bool TryExit()
        {
            if (_projectModified)
            {
                log.Info("Program is attempting to exit with an unsaved project");
                var text = string.Format("The LiveDescribe project \"{0}\" has been modified." +
                    " Do you want to save changes before closing?", _project.ProjectName);
                var result = MessageBox.Show(text, "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveProject.Execute(null);
                    return true;
                }
                else if (result == MessageBoxResult.No) //Exit but don't save
                {
                    log.Info("User has chosen exit program and not save project");
                    return true;
                }
                else
                {
                    log.Info("User has chosen not to exit program");
                    return false;
                }
            }
            else
                return true;
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
                case "FileName":
                case "IsExtendedDescription":
                case "StartWaveFileTime":
                case "EndWaveFileTime":
                case "ActualLength":
                case "StartInVideo":
                case "EndInVideo":
                case "DescriptionText":
                case "SpaceText":
                case "AudioData":
                case "Header":
                    FlagProjectAsModified();
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
            EventHandler handler = this.PlayRequested;
            if (handler != null) handler(sender, e);
        }

        private void OnPauseRequested(object sender, EventArgs e)
        {
            EventHandler handler = this.PauseRequested;
            if (handler != null) handler(sender, e);
        }

        private void OnMuteRequested(object sender, EventArgs e)
        {
            EventHandler handler = this.MuteRequested;
            if (handler != null) handler(sender, e);
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            EventHandler handler = this.MediaEnded;
            if (handler != null) handler(sender, e);
        }
        #endregion
    }
}
