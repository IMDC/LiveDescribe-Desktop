using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LiveDescribe.Model
{
    /// <summary>
    /// Contains all the data about a LiveDescribe project.
    /// </summary>
    public class Project
    {
        #region Inner Class definitions
        /// <summary>
        /// Contains the default file names and extensions for program files.
        /// </summary>
        public static class Names
        {
            /// <summary>
            /// The file extension for a project file.
            /// </summary>
            public const string ProjectExtension = ".ld";

            public const string CacheFolder = "projectCache";

            public const string WaveFormHeader = "wfheader.bin";

            public const string WaveFormFile = "waveform.bin";

            public const string DescriptionsFile = "descriptions.json";

            public const string DescriptionsFolder = "descriptions";

            public const string SpacesFile = "spaces.json";
        }

        /// <summary>
        /// Contains all the files relevant to the project.
        /// </summary>
        public class ProjectFiles
        {
            /// <summary>
            /// The file containing a list of descriptions.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile Descriptions { set; get; }

            /// <summary>
            /// The file containing all project info on disk.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile Project { set; get; }

            /// <summary>
            /// The file containing spaces.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile Spaces { set; get; }

            /// <summary>
            /// The video file used in the project.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile Video { set; get; }

            /// <summary>
            /// The file that contains .wav header data for the waveform file.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile WaveFormHeader { set; get; }

            /// <summary>
            /// The file that contains the waveform data.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile WaveForm { set; get; }

            public ProjectFiles()
            { }
        }

        /// <summary>
        /// Contains all the folders relevant to the project.
        /// </summary>
        public class ProjectFolders
        {
            /// <summary>
            /// The absolute path to the project folder.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public string Project { set; get; }

            /// <summary>
            /// The Folder containing cacheable data relating to the project.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile Cache { set; get; }

            /// <summary>
            /// The folder containing the description sound files.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public ProjectFile Descriptions { set; get; }

            public ProjectFolders()
            { }
        }
        #endregion

        /// <summary>
        /// The name of the project.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string ProjectName { set; get; }

        public ProjectFiles Files { set; get; }

        public ProjectFolders Folders { set; get; }

        /// <summary>
        /// Empty Constructor to allow for JSON serialization.
        /// </summary>
        public Project()
        { }

        /// <summary>
        /// Constructs an instance of Project. The VideoFile Property does not get set, and must be
        /// set externally.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="projectPath">Absolute path to the project folder.</param>
        public Project(string projectName, string projectPath)
        {
            ProjectName = projectName;

            string projectFolder = Path.Combine(projectPath, projectName);

            Folders = new ProjectFolders
            {
                Project = projectFolder,
                Cache = new ProjectFile(projectFolder, Names.CacheFolder),
                Descriptions = new ProjectFile(projectFolder, Names.DescriptionsFolder),
            };

            Files = new ProjectFiles
            {
                Project = new ProjectFile(Folders.Project, ProjectName + Names.ProjectExtension),
                Descriptions = new ProjectFile(Folders.Project, Names.DescriptionsFile),
                Spaces = new ProjectFile(Folders.Project, Names.SpacesFile),
                WaveFormHeader = new ProjectFile(Folders.Project, Path.Combine(Folders.Cache.RelativePath,
                    Names.WaveFormHeader)),
                WaveForm = new ProjectFile(Folders.Project, Path.Combine(Folders.Cache.RelativePath,
                    Names.WaveFormFile)),
            };
        }

        /// <summary>
        /// Constructs an instance of Project.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="videoFileName">Name and extension of the video.</param>
        /// <param name="projectPath">Absolute path to the project folder.</param>
        public Project(string projectName, string videoFileName, string projectPath) :
            this(projectName, projectPath)
        {
            Files.Video = new ProjectFile(Folders.Project, videoFileName);
        }
    }
}
