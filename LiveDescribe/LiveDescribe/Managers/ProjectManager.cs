using LiveDescribe.Events;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public event EventHandler<EventArgs<List<Description>>> DescriptionsLoaded;
        public event EventHandler<EventArgs<List<Space>>> SpacesLoaded;
        public event EventHandler<EventArgs<List<Space>>> SpacesAudioAnalysisCompleted;

        public event EventHandler<EventArgs<Project>> ProjectLoaded;

        public Project CurrentProject { private set; get; }

        #region ProjectCreation
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

                /* Create empty description and space files here, so if they are missing when
                 * opening a project, it can be noted as so.
                 */
                Log.Info("Creating descriptions file");
                FileWriter.WriteDescriptionsFile(project, new ObservableCollection<Description>());

                Log.Info("Creating spaces file");
                FileWriter.WriteSpacesFile(project, new ObservableCollection<Space>());
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
        #endregion

        #region Load Project Methods
        public void LoadProject(string projectPath, LoadingViewModel loadingViewModel)
        {
            var project = FileReader.ReadProjectFile(projectPath);
            LoadProject(project, loadingViewModel);
        }

        public void LoadProject(Project project, LoadingViewModel loadingViewModel)
        {
            InitializeDirectories(project);

            LoadDescriptions(project);
            LoadSpaces(project);

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
            Log.InfoFormat("\"{0}\" directory created", absolutePath);
        }

        private void LoadDescriptions(Project project)
        {
            var descriptions = FileReader.ReadDescriptionsFile(project);
            OnDescriptionsLoaded(descriptions);
            Log.InfoFormat("Descriptions loaded from {0}", project.ProjectName);
        }

        private void LoadSpaces(Project project)
        {
            var spaces = FileReader.ReadSpacesFile(project);
            OnSpacesLoaded(spaces);
            Log.InfoFormat("Spaces loaded from {0}", project.ProjectName);
        }

        private void StripAudioAnContinueLoadingProject(Project project, LoadingViewModel loadingViewModel)
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = true, };
            Waveform waveform = null;
            List<Space> spaceData = null;

            //Strip the audio from the given project video
            worker.DoWork += (sender, args) =>
            {
                Log.Info("Beginning to strip audio");
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

                OnSpacesAudioAnalysisCompleted(spaceData);
                Log.Info("Audio stripped and spaces found.");

                ContinueLoadingProject(project, loadingViewModel);
            };

            worker.ProgressChanged += (sender, args) => loadingViewModel.SetProgress("Importing Video",
                args.ProgressPercentage);

            loadingViewModel.SetProgress("Importing Video", 0);
            loadingViewModel.Visible = true;
            worker.RunWorkerAsync();
        }

        private void LoadWaveForm(Project project)
        {
            var header = FileReader.ReadWaveFormHeader(project);
            var audioData = FileReader.ReadWaveFormFile(project);
            project.Waveform = new Waveform(header, audioData);
            Log.InfoFormat("Waveform loaded from {0}", project.Files.WaveForm);
        }

        private void ContinueLoadingProject(Project project, LoadingViewModel loadingViewModel)
        {
            Properties.Settings.Default.WorkingDirectory = project.Folders.Project + "\\";

            CurrentProject = project;
            OnProjectLoaded(project);
            Log.InfoFormat("Project \"{0}\" loaded successfully", project.ProjectName);

            loadingViewModel.Visible = false;
        }
        #endregion

        #region Event Invokations

        private void OnDescriptionsLoaded(List<Description> descriptions)
        {
            var handler = DescriptionsLoaded;
            if (handler != null) handler(this, descriptions);
        }

        private void OnSpacesLoaded(List<Space> spaces)
        {
            var handler = SpacesLoaded;
            if (handler != null) handler(this, spaces);
        }

        private void OnSpacesAudioAnalysisCompleted(List<Space> spaces)
        {
            EventHandler<EventArgs<List<Space>>> handler = SpacesAudioAnalysisCompleted;
            if (handler != null) handler(this, spaces);
        }

        private void OnProjectLoaded(Project project)
        {
            var handler = ProjectLoaded;
            if (handler != null) handler(this, new EventArgs<Project>(project));
        }
        #endregion
    }
}
