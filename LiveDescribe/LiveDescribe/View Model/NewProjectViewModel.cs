using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.View;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace LiveDescribe.View_Model
{
    public class NewProjectViewModel : ViewModelBase
    {
        #region Fields and Properties
        private string _videoPath;
        private string _projectName;
        private string _projectPath;

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

        #region CreateWindow
        /// <summary>
        /// Creates a NewProjectView and attaches an instance of NewProjectViewModel to it. Returns
        /// a project object only when the window is closed.
        /// </summary>
        /// <returns>The newly created project object, or null if no project was created.</returns>
        public static object CreateWindow()
        {
            var viewModel = new NewProjectViewModel();
            var view = new NewProjectView(viewModel);

            var result = view.ShowDialog();

            if (result == true)
            {
                //TODO return project
                return null;
            }
            else
                return null;
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
            //TODO: This
        }
        #endregion

    }
}
