using LiveDescribe.Events;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace LiveDescribe.Managers
{
    public sealed class ProjectManager
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static readonly ProjectManager Instance = new ProjectManager();

        public event EventHandler<EventArgs<Project>> ProjectLoaded;

        public Project CurrentProject { private set; get; }

        /// <summary>
        /// Attempts to create the project file and folder
        /// </summary>
        /// <param name="project">The instance of project to initialize.</param>
        /// <returns>Whether or not initialization was successful.</returns>
        public bool TryCreateProjectFileAndFolder(Project project)
        {
            //Ensure that path is absolute
            if (!Path.IsPathRooted(project.Folders.Project))
            {
                MessageBoxFactory.ShowError("Project location must be a root path.");
                Log.Warn("Given project path is not rooted");
                return false;
            }

            if (Directory.Exists(project.Folders.Project))
            {
                var result = MessageBoxFactory.ShowWarningQuestion(
                    string.Format(UiStrings.MessageBox_Format_OverwriteProjectWarning, project.Folders.Project));

                Log.Warn("Project folder already exists");

                //Return if user doesn't agree to overwrite files.
                if (result != MessageBoxResult.Yes)
                    return false;

                Log.Info("User has decided to overwrite an existing project directory");
                FileDeleter.DeleteProject(project);
            }

            //Attempt to create files
            try
            {
                Log.Info("Creating project folder");
                Directory.CreateDirectory(project.Folders.Project);

                Log.Info("Creating project file");
                FileWriter.WriteProjectFile(project);
            }
            //TODO: Catch individual exceptions?
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(UiStrings.MessageBox_ProjectCreationError);

                Log.Error("An error occured when attempting to create files", e);

                /* TODO: Delete files on error? If we decide to do this, then only delete created
                 * files as opposed to deleting entire directory, as the latter can have
                 * disastorous consequences if user picks the wrong directory and there's an error.
                 */
                return false;
            }

            return true;
        }

        #region Load Project Methods
        public void LoadProject(string projectPath, LoadingViewModel loadingViewModel)
        {
            var project = FileReader.ReadProjectFile(projectPath);
            LoadProject(project, loadingViewModel);
        }

        public void LoadProject(Project project, LoadingViewModel loadingViewModel)
        {
            InitializeDirectories(project);

            if (!File.Exists(project.Files.WaveForm))
                StripAudioAnContinueLoadingProject(project, loadingViewModel);
            else
            {
                LoadWaveForm(project);
                ContinueLoadingProject(project, loadingViewModel);
            }
        }

        private void InitializeDirectories(Project project)
        {
            CreateDirectoryIfNotExists(project.Folders.Descriptions);
            CreateDirectoryIfNotExists(project.Folders.Cache);
        }

        private void CreateDirectoryIfNotExists(string absolutePath)
        {
            if (!Directory.Exists(absolutePath))
                Directory.CreateDirectory(absolutePath);
        }

        private void StripAudioAnContinueLoadingProject(Project project, LoadingViewModel loadingViewModel)
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = true, };
            Waveform waveform = null;
            List<Space> spaceData = null;

            //Strip the audio from the given project video
            worker.DoWork += (sender, args) =>
            {
                var audioOperator = new AudioUtility(project);
                audioOperator.StripAudio(worker);
                var waveFormData = audioOperator.ReadWavData(worker);
                var audioHeader = audioOperator.Header;
                waveform = new Waveform(audioHeader, waveFormData);
                spaceData = AudioAnalyzer.FindSpaces(waveform);
            };

            worker.RunWorkerCompleted += (sender, args) =>
            {
                project.Waveform = waveform;
                foreach (var space in spaceData)
                {
                    project.Spaces.Add(space);
                }

                ContinueLoadingProject(project, loadingViewModel);
            };

            worker.ProgressChanged += (sender, args) => loadingViewModel.SetProgress("Importing Video", args.ProgressPercentage);

            loadingViewModel.SetProgress("Importing Video", 0);
            loadingViewModel.Visible = true;
            worker.RunWorkerAsync();
        }

        private void LoadWaveForm(Project project)
        {
            var header = FileReader.ReadWaveFormHeader(project);
            var audioData = FileReader.ReadWaveFormFile(project);
            project.Waveform = new Waveform(header, audioData);
        }

        private void ContinueLoadingProject(Project project, LoadingViewModel loadingViewModel)
        {
            Properties.Settings.Default.WorkingDirectory = project.Folders.Project + "\\";

            CurrentProject = project;

            OnProjectLoaded(project);

            loadingViewModel.Visible = false;
        }
        #endregion

        #region Event Invokations

        private void OnProjectLoaded(Project project)
        {
            var handler = ProjectLoaded;
            if (handler != null) handler(this, new EventArgs<Project>(project));
        }
        #endregion
    }
}
