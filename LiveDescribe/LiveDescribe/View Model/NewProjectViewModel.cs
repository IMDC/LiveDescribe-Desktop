using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using LiveDescribe.View;
using System;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace LiveDescribe.View_Model
{
    public class NewProjectViewModel : ViewModelBase
    {
        #region Fields
        private string _videoPath;
        private string _projectName;
        private string _projectPath;

        public bool? DialogResult { set; get; }
        public Project Project { private set; get; }
        #endregion

        #region CreateWindow
        /// <summary>
        /// Creates a NewProjectView and attaches an instance of NewProjectViewModel to it.
        /// </summary>
        /// <returns>The ViewModel of the Window.</returns>
        public static NewProjectViewModel CreateWindow()
        {
            var viewModel = new NewProjectViewModel();
            var view = new NewProjectView(viewModel);

            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }
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
                RaisePropertyChanged("VideoPath");
            }
            get { return _videoPath; }
        }

        public string ProjectName
        {
            set
            {
                _projectName = value;
                RaisePropertyChanged("ProjectName");
            }
            get { return _projectName; }
        }

        public string ProjectPath
        {
            set
            {
                _projectPath = value;
                RaisePropertyChanged("ProjectPath");
            }
            get { return _projectPath; }
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
                VideoPath = fileChooser.FileName;
        }

        private void ChoosePath()
        {
            var folderChooser = new FolderBrowserDialog();

            var result = folderChooser.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                ProjectPath = folderChooser.SelectedPath;
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
        /// will be cancelled and the method will return. On success, the ProjectCreated event is
        /// invoked.
        /// </summary>
        private void CreateProject()
        {
            Project p = new Project(_projectName, Path.GetFileName(_videoPath), _projectPath);

            //Ensure that path is absolute
            if (!Path.IsPathRooted(p.ProjectFolderPath))
            {
                MessageBox.Show("Project path must be a root path.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (Directory.Exists(p.ProjectFolderPath))
            {
                var text = string.Format("The folder {0} already exists. Do you wish to overwrite its contents?",
                    p.ProjectFolderPath);
                var result = MessageBox.Show(text, "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                //Return if user doesn't agree to overwrite files.
                if (result != MessageBoxResult.Yes)
                    return;
            }

            //Attempt to create files
            try
            {
                Directory.CreateDirectory(p.ProjectFolderPath);
                Directory.CreateDirectory(p.CacheFolder);
                File.Copy(_videoPath, p.VideoFile, true);

                FileWriter.WriteProjectFile(p);
            }
            //TODO: Catch individual exceptions?
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                MessageBox.Show("An error occured while attempting to create the project.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                /* TODO: Delete files on error? If we decide to do this, then only delete created
                 * files as opposed to deleting entire directory, as the latter can have
                 * disastorous consequences if user picks the wrong directory and there's an error.
                 */
                return;
            }

            Project = p;
            OnProjectCreated();
        }
        #endregion
    }
}
