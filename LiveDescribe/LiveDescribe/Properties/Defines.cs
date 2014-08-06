namespace LiveDescribe.Properties
{
    /// <summary>
    /// Contains boolean properties that return whether or not a specific tag was defined for this
    /// build. This is to avoid conditional structures within #if preprocessor tags as Visual
    /// Studio's Intellisense does evaluate the code for #if tags that don't apply to the current
    /// build.
    /// </summary>
    internal static class Defines
    {
        public static bool Debug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool Zagga
        {
            get
            {
#if ZAGGA
                return true;
#else
                return false;
#endif
            }
        }
    }
}
