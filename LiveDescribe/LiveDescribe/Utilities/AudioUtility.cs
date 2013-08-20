﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LiveDescribe.Utilities
{
    class AudioUtility
    {
        private string _videoFile;
        private List<double> _audioData;
 
        public AudioUtility(string videoFile)
        {
            this._videoFile = videoFile;
        }

        /// <summary>
        /// Strips the audio from the given video file and outputs a wave file using FFMPEG.
        /// </summary>
        /// <param name="videoFile">the video file to operate on</param>
        /// <param name="destinationAudioFile">the striped audio file</param>
        /// <remarks>
        /// This function uses the executable FFMPEG file to handle the audio stripping
        /// </remarks>
        public void stripAudio(string destinationAudioFile)
        {
            //gets the path of the ffmpeg.exe file within the LiveDescribe solution
            var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string ffmpegPath = Path.Combine(appDirectory, "Utilities/ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                MessageBox.Show("Cannot find ffmpeg.exe" + " " + ffmpegPath);
                return;
            }

            //the parameters that are sent to the ffmpeg command
            string strParam = " -i \"" + this._videoFile + "\" -ac 2 -ab 160k -ar 44100 -f wav -vn -y \"" + destinationAudioFile + "\"";

            var ffmpeg = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = ffmpegPath,
                    Arguments = strParam,
                    CreateNoWindow = true
                }
            };

            ffmpeg.Start();
            ffmpeg.WaitForExit();

            var fileInfo = new FileInfo(destinationAudioFile);
            Console.WriteLine(fileInfo.Length);
        }
    }
}
