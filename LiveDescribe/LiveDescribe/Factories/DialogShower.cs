using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using LiveDescribe.View;
using LiveDescribe.ViewModel;

namespace LiveDescribe.Factories
{
    public static class DialogShower
    {
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

        public static SpaceRecordingViewModel SpawnSpaceRecordingView(Space selectedSpace, Project project)
        {
            var viewModel = new SpaceRecordingViewModel(selectedSpace, project);
            var view = new SpaceRecordingView(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }

        /// <summary>
        /// Creates a ExportWindowView and attaches an instance of ExportWindowViewModel to it.
        /// </summary>
        /// <returns>The ViewModel of the Window.</returns>
        public static ExportWindowViewModel SpawnExportWindowView(Project project, String videoPath, double durationSeconds,  List<Description> descriptionList, LoadingViewModel loadingViewModel)
        {
            var viewModel = new ExportWindowViewModel(project, videoPath, durationSeconds, descriptionList, loadingViewModel);
            var view = new ExportWindow(viewModel);
            viewModel.DialogResult = view.ShowDialog();

            return viewModel;
        }
    }
}
