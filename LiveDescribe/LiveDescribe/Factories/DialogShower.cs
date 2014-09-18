using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.ViewModel;
using LiveDescribe.Windows;
using System.Collections.Generic;
using ExportWindow = LiveDescribe.Windows.ExportWindow;
using ImportAudioDescriptionWindow = LiveDescribe.Windows.ImportAudioDescriptionWindow;

namespace LiveDescribe.Factories
{
    public static class DialogShower
    {
        private static AboutInfoWindow _aboutInfoWindow;

        /// <summary>
        /// Creates a NewProjectWindow and attaches an instance of NewProjectViewModel to it.
        /// </summary>
        /// <returns>The ViewModel of the Window.</returns>
        public static NewProjectViewModel SpawnNewProjectView()
        {
            var viewModel = new NewProjectViewModel();
            var view = new NewProjectWindow(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }

        /// <summary>
        /// Creates a SpaceRecordingWindow and attaches an instance of SpaceRecordingViewModel to it.
        /// </summary>
        /// <param name="selectedSpace">Space to record in.</param>
        /// <param name="project">The current LiveDescribe Project.</param>
        /// <returns>The ViewModel of the Window.</returns>
        public static SpaceRecordingViewModel SpawnSpaceRecordingView(Space selectedSpace, Project project)
        {
            var viewModel = new SpaceRecordingViewModel(selectedSpace, project);
            var view = new SpaceRecordingWindow(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }

        /// <summary>
        /// Shows the About window. If the window has been closed or hidden under another window,
        /// it will be brought back to focus.
        /// </summary>
        public static void SpawnAboutInfoView()
        {
            if (_aboutInfoWindow == null)
            {
                _aboutInfoWindow = new AboutInfoWindow();
                _aboutInfoWindow.Closed += (sender, args) => _aboutInfoWindow = null;
                _aboutInfoWindow.Show();
            }

            _aboutInfoWindow.Focus();
        }

        /// <summary>
        /// Creates a ExportWindowView and attaches an instance of ExportViewModel to it.
        /// </summary>
        /// <returns>The ViewModel of the Window.</returns>
        public static ExportViewModel SpawnExportWindowView(Project project, string videoPath, double durationSeconds, List<Description> descriptionList, LoadingViewModel loadingViewModel)
        {
            var viewModel = new ExportViewModel(project, videoPath, durationSeconds, descriptionList, loadingViewModel);
            var view = new ExportWindow(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }

        public static ImportAudioDescriptionViewModel SpawnImportAudioDescriptionView(ProjectManager projectManager, double videoDurationMilliseconds)
        {
            var viewModel = new ImportAudioDescriptionViewModel(projectManager, videoDurationMilliseconds);
            var view = new ImportAudioDescriptionWindow(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }
    }
}
