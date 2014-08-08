using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.View;
using LiveDescribe.ViewModel;
using System.Collections.Generic;

namespace LiveDescribe.Factories
{
    public static class DialogShower
    {
        private static AboutInfoView _aboutInfoView;

        /// <summary>
        /// Creates a NewProjectView and attaches an instance of NewProjectViewModel to it.
        /// </summary>
        /// <returns>The ViewModel of the Window.</returns>
        public static NewProjectViewModel SpawnNewProjectView()
        {
            var viewModel = new NewProjectViewModel();
            var view = new NewProjectView(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }

        /// <summary>
        /// Creates a SpaceRecordingView and attaches an instance of SpaceRecordingViewModel to it.
        /// </summary>
        /// <param name="selectedSpace">Space to record in.</param>
        /// <param name="project">The current LiveDescribe Project.</param>
        /// <returns>The ViewModel of the Window.</returns>
        public static SpaceRecordingViewModel SpawnSpaceRecordingView(Space selectedSpace, Project project)
        {
            var viewModel = new SpaceRecordingViewModel(selectedSpace, project);
            var view = new SpaceRecordingView(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }

        /// <summary>
        /// Shows the About window. If the window has been closed or hidden under another window,
        /// it will be brought back to focus.
        /// </summary>
        public static void SpawnAboutInfoView()
        {
            if (_aboutInfoView == null)
            {
                _aboutInfoView = new AboutInfoView();
                _aboutInfoView.Closed += (sender, args) => _aboutInfoView = null;
                _aboutInfoView.Show();
            }

            _aboutInfoView.Focus();
        }

        /// <summary>
        /// Creates a ExportWindowView and attaches an instance of ExportWindowViewModel to it.
        /// </summary>
        /// <returns>The ViewModel of the Window.</returns>
        public static ExportWindowViewModel SpawnExportWindowView(Project project, string videoPath, double durationSeconds,  List<Description> descriptionList, LoadingViewModel loadingViewModel)
        {
            var viewModel = new ExportWindowViewModel(project, videoPath, durationSeconds, descriptionList, loadingViewModel);
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
