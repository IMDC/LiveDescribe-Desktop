using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.Model
{
    /// <summary>
    /// Contains all the data about a LiveDescribe project.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// The file extension for a project file.
        /// </summary>
        public const string ProjectExtension = ".ld";

        /// <summary>
        /// The name of the project.
        /// </summary>
        public string ProjectName { private set; get; }

        /// <summary>
        /// The absolute path to the project folder.
        /// </summary>
        public string ProjectFolderPath { private set; get; }

        /// <summary>
        /// The file containing all project info on disk.
        /// </summary>
        public ProjectFile ProjectFile { private set; get; }

        /// <summary>
        /// The video file used in the project.
        /// </summary>
        public ProjectFile VideoFile { private set; get; }

        /// <summary>
        /// Constructs an instance of Project.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="videoFileName">Name and extension of the video.</param>
        /// <param name="projectPath">Absolute path to the project folder.</param>
        public Project(string projectName, string videoFileName, string projectPath)
        {
            ProjectName = projectName;

            ProjectFolderPath = Path.Combine(projectPath, projectName);

            ProjectFile = new ProjectFile(ProjectFolderPath, ProjectName + ProjectExtension);
            VideoFile = new ProjectFile(ProjectFolderPath, videoFileName);
        }
    }
}
