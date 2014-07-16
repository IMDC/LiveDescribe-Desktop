using LiveDescribe.Events;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;
using System;
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
    }
}
