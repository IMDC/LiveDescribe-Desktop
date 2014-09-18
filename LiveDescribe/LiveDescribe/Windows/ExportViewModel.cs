using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Controls.UserControls;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace LiveDescribe.Windows
{
    public class ExportViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Fields
        private string _exportName;
        private string _exportPath;
        private bool _compressAudio;
        private Project _project;
        private string _videoPath;
        private double _durationSeconds;
        private List<Description> _descriptionList;
        private LoadingViewModel _loadingViewModel;

        public bool? DialogResult { set; get; }
        public Project Project { private set; get; }
        #endregion

        #region Constructor
        public ExportViewModel(Project project, string videoPath, double durationSeconds, List<Description> descriptionList, LoadingViewModel loadingViewModel)
        {
            ChoosePathCommand = new RelayCommand(ChoosePath);
            ExportCommand = new RelayCommand(ExportProject, CanExport);

            _project = project;
            _videoPath = videoPath;
            _durationSeconds = durationSeconds;
            _descriptionList = descriptionList;
            _loadingViewModel = loadingViewModel;
        }
        #endregion

        #region events
        /// <summary>
        /// Event is raised when a project is successfully exported.
        /// </summary>
        public event EventHandler ProjectExported;

        protected virtual void OnProjectExported()
        {
            EventHandler handler = ProjectExported;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event is raised when project is finished exporting
        /// </summary>
        public event EventHandler ExportProjectCompleted;

        protected virtual void OnExportProjectCompleted()
        {
            EventHandler handler = ExportProjectCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
            _loadingViewModel.Visible = false;
        }

        #endregion

        #region Accessors
        public string ExportName
        {
            set
            {
                _exportName = value;
                RaisePropertyChanged();
            }
            get { return _exportName; }
        }

        public string ExportPath
        {
            set
            {
                _exportPath = value;
                RaisePropertyChanged();
            }
            get { return _exportPath; }
        }

        public bool CompressAudio
        {
            set
            {
                _compressAudio = value;
                RaisePropertyChanged();
            }
            get { return _compressAudio; }
        }
        #endregion

        #region Commands and Command Functions
        public RelayCommand ChoosePathCommand { private set; get; }
        public RelayCommand ExportCommand { private set; get; }


        private void ChoosePath()
        {
            var folderChooser = new FolderBrowserDialog();

            var result = folderChooser.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ExportPath = folderChooser.SelectedPath;
                Log.Info("Project path chosen: " + ExportPath);
            }
        }

        private bool CanExport()
        {
            return !string.IsNullOrWhiteSpace(_exportPath)
                && !string.IsNullOrWhiteSpace(_exportName);
        }

        /// <summary>
        /// Attempts to create a project using forminfo. If the given folder structure exists, the
        /// user will be asked for confirmation to overwrite it. On an error, the project creation
        /// will be cancelled and the method will return. On success, the ProjectCreated event is invoked.
        /// </summary>
        private void ExportProject()
        {
            //Ensure that path is absolute
            if (!Path.IsPathRooted(_exportPath))
            {
                MessageBoxFactory.ShowError("Project location must be a root path.");
                Log.Warn("Given project path is not rooted");
                return;
            }

            Log.Info("Project Exported");

            var worker = new BackgroundWorker { WorkerReportsProgress = true, };

            //Strip the audio from the given project video
            worker.DoWork += (sender, args) =>
            {
                var exportOperator = new DescriptionExportUtility(worker, _project, _videoPath, _durationSeconds, _descriptionList);
                exportOperator.exportVideoWithDescriptions(_compressAudio, _exportName, _exportPath);
            };

            //Notify subscribers of stripping completion
            worker.RunWorkerCompleted += (sender, args) =>
            {
                OnExportProjectCompleted();
            };

            worker.ProgressChanged += (sender, args) => _loadingViewModel.SetProgress("Exporting Project", args.ProgressPercentage);

            _loadingViewModel.SetProgress("Exporting Project", 0);
            _loadingViewModel.Visible = true;
            worker.RunWorkerAsync();
            OnProjectExported();
        }
        #endregion
    }
}


