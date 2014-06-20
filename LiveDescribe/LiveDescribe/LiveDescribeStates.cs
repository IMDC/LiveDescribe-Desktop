namespace LiveDescribe
{
    public enum LiveDescribeVideoStates
    {
        /// <summary>
        /// In this state when the video is playing
        /// </summary>
        PlayingVideo,

        /// <summary>
        /// In this state when the video is paused
        /// </summary>
        PausedVideo,

        /// <summary>
        /// In this state when LiveDescribe is Recording a description
        /// </summary>
        RecordingDescription,

        /// <summary>
        /// In this state when there's an error or on startup and the video file is not loaded
        /// </summary>
        VideoNotLoaded,

        /// <summary>
        /// In this state right when the video file is loaded
        /// </summary>
        VideoLoaded,
    }
}
