using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using LiveDescribe.View;
using Microsoft.TeamFoundation.Controls.WinForms;
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

        private void CreateProject()
        {
            Project = new Project(_projectName,_videoPath,_projectPath);
            OnProjectCreated();
        }
        #endregion
    }
}
