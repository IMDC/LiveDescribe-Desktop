using System.IO;
using Newtonsoft.Json;

namespace LiveDescribe.Model
{
    /// <summary>
    /// A class used to contain path strings for a specific file in a project.
    /// </summary>
    public class ProjectFile
    {
        /// <summary>
        /// Converts a ProjectFile to a string.
        /// </summary>
        /// <param name="p">A ProjectFile.</param>
        /// <returns>The absolute path of the Project file.</returns>
        public static implicit operator string(ProjectFile p)
        {
            return p.AbsolutePath;
        }

        /// <summary>
        /// The path of the project file relative to the project directory.
        /// Example is @"video.avi".
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The full path of the project file starting from its drive letter.
        /// Example is @"C:\Users\imdc\Documents\Test Projects\tProj\vid.avi".
        /// </summary>
        [JsonIgnore]
        public string AbsolutePath { get; set; }

        /// <summary>
        /// Empty Constructor to allow for JSON serialization.
        /// </summary>
        public ProjectFile()
        {}

        /// <summary>
        /// Creates a Project file with the given paths.
        /// </summary>
        /// <param name="folderPath">The absolute path of the project directory.</param>
        /// <param name="relativeFilePath">The path of the file relative to the project 
        /// directory.</param>
        public ProjectFile(string folderPath, string relativeFilePath)
        {
            RelativePath = relativeFilePath;
            AbsolutePath = Path.Combine(folderPath, relativeFilePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathToProjectFolder"></param>
        public void MakeAbsoluteWith(string pathToProjectFolder)
        {
            AbsolutePath = Path.Combine(pathToProjectFolder, RelativePath);
        }

        /// <summary>
        /// Returns the absolute path of this file as a string. It has the same result as the
        /// AbsolutePath property
        /// </summary>
        /// <returns>The absolute path of file.</returns>
        public override string ToString()
        {
            return AbsolutePath;
        }
    }
}
