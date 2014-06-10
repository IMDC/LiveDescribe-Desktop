using System;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace LiveDescribe.Model
{
    /// <summary>
    /// Contains all the data about a LiveDescribe project.
    /// </summary>
    public class Project
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

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
            [JsonIgnore]
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

        [JsonProperty(Required = Required.Always)]
        public ProjectFiles Files { set; get; }

        [JsonProperty(Required = Required.Always)]
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

        /// <summary>
        /// Sets the absolute paths of each projectfile relative to the given project file.
        /// </summary>
        /// <param name="projectFilePath">The absolute path to the project file.</param>
        public void SetAbsolutePaths(string projectFilePath)
        {
            string pathToProjectFolder = Path.GetDirectoryName(projectFilePath);

            Folders.Project = pathToProjectFolder;

            //Get a list of all the file properties
            PropertyInfo[] fileProperties = typeof(ProjectFiles).GetProperties();

            foreach (var propertyInfo in fileProperties)
            {
                //Get the actual value of the property from project
                var file = propertyInfo.GetValue(Files) as ProjectFile;

                if (file != null)
                    file.MakeAbsoluteWith(pathToProjectFolder);
            }

            PropertyInfo[] folderProperties = typeof(ProjectFolders).GetProperties();

            foreach (var propertyInfo in folderProperties)
            {
                var folder = propertyInfo.GetValue(Folders) as ProjectFile;

                if (folder != null)
                    folder.MakeAbsoluteWith(pathToProjectFolder);
            }
        }

        /// <summary>
        /// Generates a file name and absolue path for a description. The file name will be in the
        /// form of projectname_desc_yyMMddHHmmssfff.wav, where yyMMddHHmmssfff a timestamp from
        /// the current time.
        /// </summary>
        /// <returns></returns>
        public string GenerateDescriptionFilePath()
        {
            //TODO: Move this functionality elsewhere
            string fileName = string.Format("{0}_desc_{1}.wav", 
                ProjectName, DateTime.Now.ToString("yyMMddHHmmssfff"));

            string path = Path.Combine(Folders.Descriptions, fileName);

            return path;
        }
    }
}
