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

        public const string CacheFolderName = "projectCache";

        public const string WaveFormFileName = "waveform.bin";

        public const string DescriptionsFolderName = "descriptions";

        /// <summary>
        /// The name of the project.
        /// </summary>
        public string ProjectName { set; get; }

        /// <summary>
        /// The absolute path to the project folder.
        /// </summary>
        public string ProjectFolderPath { set; get; }

        /// <summary>
        /// The Folder containing cacheable data relating to the project
        /// </summary>
        public ProjectFile CacheFolder { set; get; }

        public ProjectFile DescriptionsFolder { set; get; }

        /// <summary>
        /// The file containing all project info on disk.
        /// </summary>
        public ProjectFile ProjectFile { set; get; }

        /// <summary>
        /// The video file used in the project.
        /// </summary>
        public ProjectFile VideoFile { set; get; }

        /// <summary>
        /// The file that contains the waveform data.
        /// </summary>
        public ProjectFile WaveFormFile { set; get; }

        /// <summary>
        /// Empty Constructor to allow for JSON serialization.
        /// </summary>
        public Project()
        { }

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

            //Folders
            CacheFolder = new ProjectFile(ProjectFolderPath,CacheFolderName);
            DescriptionsFolder = new ProjectFile(ProjectFolderPath,DescriptionsFolderName);

            //Files
            ProjectFile = new ProjectFile(ProjectFolderPath, ProjectName + ProjectExtension);
            VideoFile = new ProjectFile(ProjectFolderPath, videoFileName);
            WaveFormFile = new ProjectFile(ProjectFolderPath, Path.Combine(CacheFolder.RelativePath,
                WaveFormFileName));
        }
    }
}
