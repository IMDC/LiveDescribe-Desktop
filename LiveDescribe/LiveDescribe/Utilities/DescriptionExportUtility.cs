using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using System.IO;
using System.Reflection;
using System.Diagnostics;
namespace LiveDescribe.Utilities
{
    class DescriptionExportUtility
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private string _videoFile;
        private double _videoDurationSeconds;
        private List<Description> _descriptionList;
        private string _ffmpegPath;
        private Project _project;
        #endregion

        #region Constructors
        public DescriptionExportUtility(Project project, string videoFile, double videoDurationSeconds, List<Description> descriptionList)
        {
            _project = project;
            _videoFile = videoFile;
            _videoDurationSeconds = videoDurationSeconds;
            _descriptionList = descriptionList;
            //gets the path of the ffmpeg.exe file within the LiveDescribe solution
            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _ffmpegPath = Path.Combine(appDirectory, "Utilities/ffmpeg.exe");

            if (!File.Exists(_ffmpegPath))
            {
                Log.Error("ffmpeg.exe can not be found at " + _ffmpegPath);
                //need to do error handling
            }
        }
        #endregion

        /// <summary>
        /// Exports the recorded descriptions and adds them to the video associated with 
        /// the project. This method is called from a Relay Command(ExportWithDescriptions) 
        /// in MainWindowViewModel.cs
        /// </summary>
        public void exportVideoWithDescriptions()
        {
            if (_descriptionList.Count > 0)
            {
                string audioTrack = convertAudioToMP3(createDescriptionTrack());
                mixAudioVideo(audioTrack, _videoFile);
            }
            else
            {
                Log.Warn("No descriptions to be exported");
            }
        }

        /// <summary>
        /// Creates a single file that contains all description tracks
        /// </summary>
        /// <returns>Absolute path to the file created</returns>
        private string createDescriptionTrack()
        {
            Log.Info("Preparing to Create Description Audio");
            List<string> concat_list = new List<string>();
            List<Description> descriptions = new List<Description>(_descriptionList);
            string outFileName =  _project.Folders.Project + "\\descriptions\\combined_description_track.wav";
           
            descriptions.Sort((x, y) => x.StartInVideo.CompareTo(y.StartInVideo));
            string init_silence = createBlankAudio(descriptions[0].StartInVideo / 1000); //create silence track for the begining of the description track
            concat_list.Add(init_silence);

            for ( int i = 0; i < descriptions.Count; i++ )
            {
                double delta;

                if (i != descriptions.Count - 1) //not the last item
                {
                    delta = descriptions[i + 1].StartInVideo - (descriptions[i].StartInVideo / 1000);
                }
                else
                {
                    delta = _videoDurationSeconds - (descriptions[i].StartInVideo / 1000);
                }
                concat_list.Add(appendSilence(descriptions[i].AudioFile, delta));
            }

            string command = "";

            foreach (String file in concat_list)
            {
                command +=  " -i \"" + file + "\"";
            }

            command += " -filter_complex concat=n=" + concat_list.Count + ":v=0:a=1 -y " + "\""
                        + outFileName + "\"";
            ffmpegCommand(command);

            #region Delete temp files
            try
            {
                foreach (string file in concat_list)
                {
                    File.Delete(file);
                }

                File.Delete(init_silence);
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error("Error removing temp files: " + ex);
            }
            #endregion

            return outFileName;
        }

        /// <summary>
        /// Creates a silent audio file
        /// </summary>
        /// <param name="duration">The length, in seconds, to make the audio file</param>
        /// <returns>Absolute path to the file created</returns>
        private string createBlankAudio(double duration)
        {
            Log.Info("Creating blank audio file");
            string command = " -f lavfi -i aevalsrc=0:0::duration=" + duration +
                             " -ab 320k -y \"" + _project.Folders.Project + "\\descriptions\\init_silence.wav\"";
            ffmpegCommand(command);

            Log.Info("Blank audio file created");
            return _project.Folders.Project + "\\descriptions\\init_silence.wav";
        }

        /// <summary>
        /// Appends silence to an existing audio file
        /// </summary>
        /// <param name="audioFile">Path to the audio file</param>
        /// <param name="duration">Seconds of silence to be appended</param>
        /// <returns>Absolute path to the file created</returns>
        private string appendSilence(string audioFile, double duration)
        {
            Log.Info("Appending " + duration + " seconds of silence to " + audioFile);
            string[] separator = new string[] {"\\"};
            string[] filePathSplit;
            string outFileName;

            //rename the file to be padded with silence
            filePathSplit = audioFile.Split(separator, StringSplitOptions.None);
            filePathSplit[filePathSplit.Length - 1] = "export_" + filePathSplit[filePathSplit.Length - 1];
            outFileName = String.Join("\\", filePathSplit);


            string command = " -i \"" + audioFile + "\" -filter_complex aevalsrc=0::d=" + duration 
                            + "[silence];[0:a][silence]concat=n=2:v=0:a=1[out] -map [out] -y \"" 
                            + outFileName +"\"";
            ffmpegCommand(command);

            return outFileName;
        }

        /// <summary>
        /// Mixes an audio track with a video file
        /// </summary>
        /// <param name="audioPath"></param>
        /// <param name="videoPath"></param>
        /// <returns>Absolute path to the file created</returns>
        private string mixAudioVideo(string audioPath, string videoPath)
        {
            Log.Info("Mixing " + audioPath + " with " + videoPath);
            string[] separator = new string[] {"\\"};
            string[] filePathSplit;
            string outFileName;

            //rename the file 
            filePathSplit = videoPath.Split(separator, StringSplitOptions.None);
            filePathSplit[filePathSplit.Length - 1] = "export_" + filePathSplit[filePathSplit.Length - 1];
            outFileName = String.Join("\\", filePathSplit);

            string command = " -i \"" + videoPath + "\" -i \"" + audioPath + "\" -c copy -map 0:0 -map 0:1 -map 1:0 -y \"" + outFileName + "\"";
            ffmpegCommand(command);

            return outFileName;
        }

        /// <summary>
        ///             NOT YET IMPLEMENTED!
        /// </summary>
        /// <param name="videoPath"></param>
        /// <returns></returns>
        private string stripVideoAudio(string videoPath)
        {
            string[] separator = new string[] { "\\" };
            string[] filePathSplit;
            string outFileName;

            //rename the file 
            filePathSplit = videoPath.Split(separator, StringSplitOptions.None);
            filePathSplit[filePathSplit.Length - 1] = "export_stripped_video.wav";
            outFileName = String.Join("\\", filePathSplit);



            return outFileName;
        }


        /// <summary>
        ///              NOT YET IMPLEMENTED!
        /// </summary>
        /// <param name="audioPath"></param>
        /// <param name="volume"></param>
        private void changeAudioVolume(string audioPath, double volume)
        { 

        }

        /// <summary>
        /// Converts audio files to MP3 format, removes old file.
        /// </summary>
        /// <param name="audioPath"></param>
        /// <returns>Absolute path to the file created</returns>
        private string convertAudioToMP3(string audioPath)
        {
            string[] separator = new string[] { "\\" };
            string[] filePathSplit;
            string outFileName;

            //rename the file 
            filePathSplit = audioPath.Split(separator, StringSplitOptions.None);
            filePathSplit[filePathSplit.Length - 1] = filePathSplit[filePathSplit.Length - 1].Split(new string[] { "." }, StringSplitOptions.None)[0] + ".mp3";
            outFileName = String.Join("\\", filePathSplit);

            string command = string.Format(" -i \"{0}\" -f mp3 {1}", audioPath, outFileName);
            ffmpegCommand(command);

            try
            {
               File.Delete(audioPath); 
            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error("Error removing files: " + ex);
            }

            return outFileName;
        }

        /// <summary>
        /// Executes an ffmpeg command
        /// </summary>
        /// <param name="command">String representation of an ffmpeg command</param>
        private void ffmpegCommand(string command)
        {
            var ffmpeg = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = _ffmpegPath,
                    Arguments = command,
                    CreateNoWindow = true
                }
            };
            ffmpeg.Start();

            #region FFMPEG Output
            StreamReader input = ffmpeg.StandardError;

            try
            {
                while (!input.EndOfStream)
                {
                    string text = input.ReadLine();
                    Console.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured during ffmpeg execution", ex);
            }
            #endregion

            ffmpeg.WaitForExit();
        }
    }
}
