using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe
{
    public enum LiveDescribeStates
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
        VideoLoaded
    }
}
