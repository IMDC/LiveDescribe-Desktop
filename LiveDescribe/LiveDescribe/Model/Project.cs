using System;
using System.Collections.Generic;
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
        public string ProjectName { set; get; }

        /// <summary>
        /// The absolute path to the video to import.
        /// </summary>
        public string VideoPath { set; get; }

        /// <summary>
        /// The absolute path to the project folder.
        /// </summary>
        public string ProjectPath { set; get; }

        public Project(string projectName, string videoPath, string projectPath)
        {
            this.ProjectName = projectName;
            this.VideoPath = VideoPath;
            this.ProjectPath = projectPath;
        }
    }
}
