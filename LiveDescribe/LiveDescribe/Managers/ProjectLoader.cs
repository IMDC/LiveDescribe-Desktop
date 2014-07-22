using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace LiveDescribe.Managers
{
    public class ProjectLoader
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ProjectCreation
        /// <summary>
        /// Attempts to create the project file and folder
        /// </summary>
        /// <param name="project">The instance of project to initialize.</param>
        /// <returns>Whether or not initialization was successful.</returns>
        public static void InitializeProjectDirectory(Project project)
        {
            //Ensure that path is absolute
            if (!Path.IsPathRooted(project.Folders.Project))
            {
                MessageBoxFactory.ShowError("Project location must be a root path.");
                Log.Warn("Given project path is not rooted");
                throw new ArgumentException("Given project path is not rooted");
            }

            if (Directory.Exists(project.Folders.Project))
            {
                var result = MessageBoxFactory.ShowWarningQuestion(
                    string.Format(UiStrings.MessageBox_Format_OverwriteProjectWarning, project.Folders.Project));

                Log.Warn("Project folder already exists");

                //Return if user doesn't agree to overwrite files.
                if (result != MessageBoxResult.Yes)
                    throw new OperationCanceledException("User decided not to overwrite already existing project");

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
                throw;
            }
        }
        #endregion
    }
}
