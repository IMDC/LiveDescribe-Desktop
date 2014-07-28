namespace LiveDescribe.Model
{
    /// <summary>
    /// Represents a File or Directory that is associated with a name.
    /// </summary>
    public class NamedFilePath
    {
        /// <summary>
        /// The name associated with the path.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path.
        /// </summary>
        public string Path { get; set; }
    }
}
