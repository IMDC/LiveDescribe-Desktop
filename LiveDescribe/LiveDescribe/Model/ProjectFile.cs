using System;
using Newtonsoft.Json;
using System.IO;

namespace LiveDescribe.Model
{
    /// <summary>
    /// A class used to contain path strings for a specific file in a project.
    /// </summary>
    public class ProjectFile
    {
        #region Factory Methods
        /// <summary>
        /// Creates a ProjectFile with a given relative path, and a base path that will contain the
        /// relative path. Use this when given a relative path but not an absolute path.
        /// </summary>
        /// <param name="relativePath">A non-rooted path.</param>
        /// <param name="basePath">A rooted path that will serve as the parent folder to the
        /// relative path.</param>
        /// <returns>A ProjectFile with a set AbsolutePath and RelativePath.</returns>
        public static ProjectFile FromRelativePath(string relativePath, string basePath)
        {
            var pf = new ProjectFile { RelativePath = relativePath };
            pf.MakeAbsoluteWith(basePath);

            return pf;
        }

        /// <summary>
        /// Creates a ProjectFile with a given absolute(rooted) path, and a base path. The
        /// ProjectFile's RelativePath will be set relative to the given base path. Use this when
        /// given an absolute path but not a relative path. Note that the basePath does not have to
        /// contain the relative path.
        /// </summary>
        /// <param name="absolutePath">A rooted path.</param>
        /// <param name="basePath">A base path to make the file relative to.</param>
        /// <returns></returns>
        public static ProjectFile FromAbsolutePath(string absolutePath, string basePath)
        {
            var pf = new ProjectFile { AbsolutePath = absolutePath };
            pf.MakeRelativeTo(basePath);

            return pf;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Converts a ProjectFile to a string.
        /// </summary>
        /// <param name="pf">A ProjectFile.</param>
        /// <returns>The absolute path of the Project file.</returns>
        public static implicit operator string(ProjectFile p)
        {
            return p.AbsolutePath;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The path of the project file relative to the project directory. Example is @"video.avi".
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The full path of the project file starting from its drive letter. Example is
        /// @"C:\Users\imdc\Documents\Test Projects\tProj\vid.avi".
        /// </summary>
        [JsonIgnore]
        public string AbsolutePath { get; set; }
        #endregion

        /// <summary>
        /// Empty Constructor to allow for JSON serialization. The static factory constructor
        /// methods should be favoured over this method.
        /// </summary>
        public ProjectFile()
        { }

        #region Methods
        /// <summary>
        /// Sets the absolute path of the project file based on the combination of the project
        /// folder path and the file's relative path.
        /// </summary>
        /// <param name="pathToProjectFolder"></param>
        public void MakeAbsoluteWith(string pathToProjectFolder)
        {
            /* Note: Uris don't treat the last level of a path as a directory unless it is followed
             * by a slash (or an escaped backslash for a windows path), so one must be added for
             * paths obtained by using folderchooser or the Path class.
             */
            var baseUri = new Uri(pathToProjectFolder + "\\", UriKind.Absolute);
            var relativeUri = new Uri(RelativePath, UriKind.Relative);
            var absoluteUri = new Uri(baseUri, relativeUri);
            AbsolutePath = absoluteUri.LocalPath;
        }

        /// <summary>
        /// Sets the RelativePath of the project file based on the difference of the file's
        /// AbsolutePath and the given basePath.
        /// </summary>
        /// <param name="basePath"></param>
        public void MakeRelativeTo(string basePath)
        {
            var baseUri = basePath.EndsWith("\\")
                ? new Uri(basePath, UriKind.Absolute)
                : new Uri(basePath + "\\", UriKind.Absolute);
            var absoluteUri = new Uri(AbsolutePath, UriKind.Absolute);

            var relativeUri = baseUri.MakeRelativeUri(absoluteUri);

            RelativePath = relativeUri.ToString();
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
        #endregion
    }
}
