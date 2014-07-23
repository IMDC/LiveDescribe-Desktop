using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Factories;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace LiveDescribe.ViewModel
{
    public class NewProjectViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Fields
        private string _videoPath;
        private string _projectName;
        private string _projectPath;
        private bool _copyVideo;

        public bool? DialogResult { set; get; }
        public Project Project { private set; get; }
        #endregion

        #region Constructor
        public NewProjectViewModel()
        {
            ChooseVideoCommand = new RelayCommand(ChooseVideo);
            ChoosePathCommand = new RelayCommand(ChoosePath);
            CreateProjectCommand = new RelayCommand(CreateProject, CanCreateProject);
        }
        #endregion

        #region events
        /// <summary>
        /// Event is raised when a project is successfully created.
        /// </summary>
        public event EventHandler ProjectCreated;

        protected virtual void OnProjectCreated()
        {
            EventHandler handler = ProjectCreated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Accessors
        public string VideoPath
        {
            set
            {
                _videoPath = value;
                RaisePropertyChanged();
            }
            get { return _videoPath; }
        }

        public string ProjectName
        {
            set
            {
                _projectName = value;
                RaisePropertyChanged();
            }
            get { return _projectName; }
        }

        public string ProjectPath
        {
            set
            {
                _projectPath = value;
                RaisePropertyChanged();
            }
            get { return _projectPath; }
        }

        public bool CopyVideo
        {
            set
            {
                _copyVideo = value;
                RaisePropertyChanged();
            }
            get { return _copyVideo; }
        }
        #endregion

        #region Commands and Command Functions
        public RelayCommand ChooseVideoCommand { private set; get; }
        public RelayCommand ChoosePathCommand { private set; get; }
        public RelayCommand CreateProjectCommand { private set; get; }

        private void ChooseVideo()
        {
            var fileChooser = new OpenFileDialog();

            bool? dialogSuccess = fileChooser.ShowDialog();

            if (dialogSuccess == true)
            {
                VideoPath = fileChooser.FileName;
                Log.Info("Video file chosen: " + VideoPath);
            }
        }

        private void ChoosePath()
        {
            var folderChooser = new FolderBrowserDialog();

            var result = folderChooser.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ProjectPath = folderChooser.SelectedPath;
                Log.Info("Project path chosen: " + ProjectPath);
            }
        }

        private bool CanCreateProject()
        {
            return !string.IsNullOrWhiteSpace(_videoPath)
                && !string.IsNullOrWhiteSpace(_projectName)
                && !string.IsNullOrWhiteSpace(_projectPath);
        }

        /// <summary>
        /// Attempts to create a project using forminfo. If the given folder structure exists, the
        /// user will be asked for confirmation to overwrite it. On an error, the project creation
        /// will be cancelled and the method will return. On success, the ProjectCreated event is invoked.
        /// </summary>
        private void CreateProject()
        {
            Log.Info(CopyVideo
                ? "Attempting to create a project with a copied video"
                : "Attempting to create a project with a video located outside of project");

            var project = new Project(ProjectName, ProjectPath, VideoPath, CopyVideo);

            try { ProjectLoader.InitializeProjectDirectory(project); }
            catch { return; }

            Project = project;

            Log.Info("Project Created");
            OnProjectCreated();
        }
        #endregion
    }
}
