using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;

namespace LiveDescribe.Utilities
{
    public static class FileDeleter
    {
        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Deletes all the files in a project folder defined by a project object, if they exist in
        /// the file system.
        /// </summary>
        /// <param name="project">The project to delete.</param>
        public static void DeleteProject(Project project)
        {
            //Get a list of all the file properties
            PropertyInfo[] fileProperties = typeof(Project.ProjectFiles).GetProperties();

            //Iterate through the list of file properties
            foreach (var propertyInfo in fileProperties)
            {
                //Get the actual value of the property from project
                var f = propertyInfo.GetValue(project.Files) as ProjectFile;

                //If f was a projectFile and the file exists, delete it
                if (f != null && File.Exists(f))
                {
                    File.Delete(f);
                    log.Info("Deleting project file " + f.RelativePath);
                }
            }

            //Do the same for all the folders
            PropertyInfo[] folderProperties = typeof(Project.ProjectFolders).GetProperties();

            foreach (var propertyInfo in folderProperties)
            {
                var f = propertyInfo.GetValue(project.Folders) as ProjectFile;

                if (f != null && Directory.Exists(f))
                {
                    //Delete folders only if empty
                    var files = Directory.GetFiles(f);
                    if (files.Length == 0)
                    {
                        Directory.Delete(f, false);
                        log.Info("Deleting project folder " + f.RelativePath);
                    }
                }

                //TODO delete .wav description files.
            }
        }
    }
}
