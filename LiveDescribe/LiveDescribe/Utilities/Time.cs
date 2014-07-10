namespace LiveDescribe.Utilities
{
    /* In here are a few static classes that contain time conversion values to help eliminate magic
     * numbers from the code and make it more informative.
     */

    public static class Seconds
    {
        public const int PerMinute = 60;
    }

    public static class Milliseconds
    {
        public const int PerSecond = 1000;
        public const int PerMinute = Seconds.PerMinute * PerSecond;
    }
}
