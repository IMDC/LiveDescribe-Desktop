using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using LiveDescribe.Factories;
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
        private BackgroundWorker _progressWorker;
        private int _operations;
        private double _progress;
        #endregion

        #region Constructors
        public DescriptionExportUtility(BackgroundWorker progressWorker, Project project, string videoFile, double videoDurationSeconds, List<Description> descriptionList)
        {
            _project = project;
            _videoFile = videoFile;
            _videoDurationSeconds = videoDurationSeconds;
            _descriptionList = descriptionList;
            _progressWorker = progressWorker;
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
        public void exportVideoWithDescriptions(bool compressAudio, string exportName, string exportPath)
        {
            if (_descriptionList.Count > 0)
            {
                _operations = compressAudio == true ? 6 : 5;
                _progress = 0;

                string audioTrack = createDescriptionTrack();
                string videoAudio = stripVideoAudio(_videoFile);
                audioTrack = muxAudioFiles(videoAudio, audioTrack);
               
                if (compressAudio)
                {   
                    audioTrack = convertAudioToMP3(audioTrack);   
                }

                mixAudioVideo(audioTrack, _videoFile, exportName, exportPath);

                #region Remove Temp Files
                try
                {
                    File.Delete(audioTrack);
                }
                catch (DirectoryNotFoundException ex)
                {
                    Log.Error("Error removing files: " + ex);
                }
                #endregion
            }
            else
            {
                Log.Warn("No descriptions to be exported");
            }
        }

        /// <summary>
        /// Exports the recorded descriptions (extended and regular) and adds them to the video associated with 
        /// the project. This method is called from a Relay Command(ExportWithDescriptions) 
        /// in MainWindowViewModel.cs
        /// </summary>
        /// <param name="compressAudio"></param>
        /// <param name="exportName"></param>
        /// <param name="exportPath"></param>
        public void exportAudioWithExtendedDescriptions(bool compressAudio, string exportName, string exportPath)
        {
            if (_descriptionList.Count > 0)
            {
                string audioTrack = createDescriptionTrack();
                string videoAudio = stripVideoAudio(_videoFile);

                audioTrack = muxAudioFiles(videoAudio, audioTrack); //audio with non-extended descriptions

                string finalTrack = addExtendedDescriptionsToAudio(audioTrack, exportName, exportPath);

                if (compressAudio)
                {
                    convertAudioToMP3(finalTrack);
                }

                #region Remove Temp Files
                try
                {
                    File.Delete(audioTrack);
                }
                catch (DirectoryNotFoundException ex)
                {
                    Log.Error("Error removing files: " + ex);
                }
                #endregion
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
            string init_silence;
            List<string> concat_list = new List<string>();
            List<Description> descriptions = new List<Description>(_descriptionList);
            string outFileName =  _project.Folders.Project + "\\descriptions\\combined_description_track.wav";
           
            descriptions.Sort((x, y) => x.StartInVideo.CompareTo(y.StartInVideo));

            int firstDescriptionIndex;
            //find the first non-extended description
            for ( firstDescriptionIndex = 0; firstDescriptionIndex < descriptions.Count; firstDescriptionIndex++ )
            {
                if (!descriptions[firstDescriptionIndex].IsExtendedDescription)
                    break;
            }

            if (descriptions[firstDescriptionIndex].StartInVideo > 0)
            {
                init_silence = createBlankAudio(descriptions[firstDescriptionIndex].StartInVideo / 1000); //create silence track for the begining of the description track
                concat_list.Add(init_silence);
            }
           
            for ( int i = 0; i < descriptions.Count; i++ )
            {
                if (!descriptions[i].IsExtendedDescription)
                {
                    double delta;

                    if (i != descriptions.Count - 1) //not the last item
                    {
                        delta = (descriptions[i + 1].StartInVideo - descriptions[i].EndInVideo) / 1000;

                    }
                    else
                    {
                        delta = 0; // _videoDurationSeconds - (descriptions[i].StartInVideo / 1000);
                    }

                    concat_list.Add(appendSilence(descriptions[i].AudioFile, delta));

                    #region progress update
                    double local_progress = (((i + 1) * 100) / descriptions.Count) / _operations;
                    _progressWorker.ReportProgress((int)Math.Round(_progress + local_progress));
                    #endregion
                }
                else //update origress anyway so that no jumping occurs in the progress bar
                {
                    #region progress update
                    double local_progress = (((i + 1) * 100) / descriptions.Count) / _operations;
                    _progressWorker.ReportProgress((int)Math.Round(_progress + local_progress));
                    #endregion
                }
            }

            _progress += (100 / _operations);

            #region Prepare FFMPEG Command
            string command = "";
            string sub_command = "";
            int j = 0;

            foreach (String file in concat_list)
            {
                command +=  " -i \"" + file + "\"";
                sub_command += "[" + j + ":0]";
                j++;
            }

            command += " -filter_complex " + sub_command + "concat=n=" + concat_list.Count + ":v=0:a=1[out] -map [out] -y " + "\""
                        + outFileName + "\"";
            ffmpegCommand(command, true);
            #endregion

            #region Delete temp files
            try
            {
                foreach (string file in concat_list)
                {
                    File.Delete(file);
                }

            }
            catch (DirectoryNotFoundException ex)
            {
                Log.Error("Error removing temp files: " + ex);
            }
            #endregion

            return outFileName;
        }

        /// <summary>
        /// Adds the extended descriptions of the project to the audio file which has the
        /// regular descriptions already mixed in
        /// </summary>
        /// <param name="audioTrack"></param>
        /// <param name="exportName"></param>
        /// <param name="exportPath"></param>
        private string addExtendedDescriptionsToAudio(string audioTrack, string exportName, string exportPath)
        {
            List<string> concat_list = new List<string>();
            List<string> temp_file_list = new List<string>();
            List<Description> descriptions = new List<Description>(_descriptionList); 
            descriptions.Sort((x, y) => x.StartInVideo.CompareTo(y.StartInVideo));

            string outFileName = string.Format("{0}\\{1}.wav", exportPath, exportName);

            //divide the audio track into sections between the extended descriptions
            for (int i = 0; i < descriptions.Count; i++)
            {
                if (descriptions[i].IsExtendedDescription)
                {
                    double startTime = descriptions[i].StartInVideo / 1000;
                    double descriptionDuration = descriptions[i].Duration / 1000;
                    string chunkName = _project.Folders.Project + "\\descriptions\\partial_track_" + i + ".wav";
                    concat_list.Add(chunkName);
                    temp_file_list.Add(chunkName);
                    concat_list.Add(descriptions[i].AudioFile);
                    string sliceCommand = String.Format(" -i {0} -ss {1} -t {2} -c copy -map 0 {3}",
                                        audioTrack, startTime, descriptionDuration, chunkName);
                    ffmpegCommand(sliceCommand, false);
                }

                if ( i == descriptions.Count - 1 ) //if it is the last one, slice the last bit of audio
                {
                    double startTime = descriptions[i].StartInVideo / 1000;
                    double remainingTime = _videoDurationSeconds - startTime;
                    string chunkName = _project.Folders.Project + "\\descriptions\\partial_track_" + (i + 1) + ".wav";
                    concat_list.Add(chunkName);
                    temp_file_list.Add(chunkName);
                    string sliceCommand = String.Format(" -i {0} -ss {1} -t {2} -c copy -map 0 {3}",
                                        audioTrack, startTime, remainingTime, chunkName);
                    ffmpegCommand(sliceCommand, false);
                }

                #region progress update
                double local_progress = (((i + 1) * 100) / descriptions.Count) / _operations;
                _progressWorker.ReportProgress((int)Math.Round(_progress + local_progress));
                #endregion
            }

            _progress += (100 / _operations);


            #region Prepare FFMPEG Command
            string command = "";
            string sub_command = "";
            int j = 0;

            foreach (String file in concat_list)
            {
                command = String.Format("{0} -i {1} ", command, file);
                sub_command += "[" + j + ":0]";
                sub_command = String.Format("{0} [{1}:0] ", sub_command, j);
                j++;
            }

            command += " -filter_complex " + sub_command + "concat=n=" + concat_list.Count + ":v=0:a=1[out] -map [out] -y " + "\""
                        + outFileName + "\"";
            ffmpegCommand(command, true);
            #endregion

            #region Delete temp files
            try
            {
                foreach (string file in temp_file_list)
                {
                    File.Delete(file);
                }

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
            ffmpegCommand(command, false);

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
            ffmpegCommand(command, false);

            return outFileName;
        }

        /// <summary>
        /// Mixes an audio track with a video file
        /// </summary>
        /// <param name="audioPath"></param>
        /// <param name="videoPath"></param>
        /// <returns>Absolute path to the file created</returns>
        private string mixAudioVideo(string audioPath, string videoPath, string exportName, string exportPath)
        {
            Log.Info("Mixing " + audioPath + " with " + videoPath);
            string[] separator = new string[] {"\\"};
            string[] filePathSplit;
            string outFileName;

            //rename the file 
            filePathSplit = videoPath.Split(separator, StringSplitOptions.None);
            string ext = filePathSplit[filePathSplit.Length - 1].Split(new string[] {"."}, StringSplitOptions.None).Last<string>();
            filePathSplit[filePathSplit.Length - 1] = string.Format("{0}.{1}", exportName, ext);

            //outFileName = String.Join("\\", filePathSplit);
            outFileName = string.Format("{0}\\{1}.{2}", exportPath, exportName, ext);
 
            string command = " -i \"" + videoPath + "\" -i \"" + audioPath + "\" -c:v copy  -map 0:0 -map 1:0 -y \"" + outFileName + "\"";
            ffmpegCommand(command, true);

            return outFileName;
        }

        /// <summary>
        /// Mix together given audio files and take the output to
        /// have the length of the first audio file
        /// </summary>
        /// <param name="videoAudio"></param>
        /// <param name="descriptionAudio"></param>
        /// <returns>Absolute path to the file created</returns>
        private string muxAudioFiles(string videoAudio, string descriptionAudio)
        {
            string outFile = _project.Folders.Descriptions + "\\combined_description_audio.wav";
            string command = string.Format(" -i \"{0}\" -i \"{1}\" -filter_complex amix=inputs=2:duration=first \"{2}\"",
                                       videoAudio, descriptionAudio, outFile);
            ffmpegCommand(command, true);

            return outFile;
        }

        /// <summary>
        ///      
        /// </summary>
        /// <param name="videoPath"></param>
        /// <returns>Absolute path to the file created</returns>
        private string stripVideoAudio(string videoPath)
        {
            Log.Info("Stripping audio from: " + videoPath);
            string[] separator = new string[] { "\\" };
            string[] filePathSplit;
            string outFileName;

            //rename the file 
            filePathSplit = videoPath.Split(separator, StringSplitOptions.None);
            filePathSplit[filePathSplit.Length - 1] = "export_stripped_video.wav";
            outFileName = String.Join("\\", filePathSplit);

            string command = string.Format(" -i \"{0}\" -ac 2 -ab 160k -ar 44100 -f wav -vn -y \"{1}\"", videoPath, outFileName);
            ffmpegCommand(command, true);

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

            string command = string.Format(" -i \"{0}\" -f mp3 \"{1}\"", audioPath, outFileName);
            ffmpegCommand(command, true);

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
        private void ffmpegCommand(string command, bool reportProgress)
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
            if (reportProgress)
            {
                double totalTime = 0;
                double currentTime = 0;

                //stream reader used to parse the output of the ffmpeg process
                StreamReader input = ffmpeg.StandardError;

                Log.Info("Attempting to parse ffmpeg output");
                /* Parsing the output of ffmpeg to obtain the total time and the current time
                 to  calculate a percentage whose value is used to update the progress bar*/
                try
                {
                    while (!input.EndOfStream)
                    {
                        string text = input.ReadLine();
                        Console.WriteLine("FFMPEG: " + text);
                        string word = "";
                        for (int i = 0; i < text.Length; i++)
                        {
                            word += text[i];
                            if (text[i] == ' ')
                            {
                                if (word.Equals("Duration: "))
                                {
                                    int currentIndex = i + 1;
                                    string time = "";

                                    for (int j = currentIndex; j < currentIndex + 11; j++)
                                    {
                                        time += text[j];
                                    }

                                    totalTime = GetTime(time);
                                }
                                word = "";
                            }

                            if (text[i] == '=')
                            {
                                if (word.Equals("time="))
                                {
                                    int currentIndex = i + 1;
                                    string time = "";

                                    for (int j = currentIndex; j < currentIndex + 11; j++)
                                    {
                                        time += text[j];
                                    }

                                    currentTime = GetTime(time);
                                }
                            }
                        }

                        //updates the progress bar given that the total time is not zero
                        if (totalTime != 0)
                        {
                            int percentComplete = Convert.ToInt32((currentTime / totalTime) * 100);
                            if (percentComplete <= 100)
                            {
                                #region progress update
                                double local_progress = percentComplete / _operations;
                                _progressWorker.ReportProgress((int)Math.Round(_progress + local_progress));
                                //((int)Math.Round(_progress + local_progress));
                                #endregion
                            }
                        }
                    }
                    _progress += (100 / _operations);
                }
                catch (Exception ex)
                {
                    MessageBoxFactory.ShowError(ex.Message);
                    Log.Error("An error occured during ffmpeg Exporting", ex);
                }
            }
           
            #endregion

            ffmpeg.WaitForExit();
        }


        /// <summary>
        /// Convert from ffmpeg time to seconds
        /// </summary>
        /// <param name="time">the time in ffmpeg format HH:MM:SS</param>
        /// <returns></returns>
        private double GetTime(string time)
        {
            double hours = Convert.ToDouble(time.Substring(0, 2)) * 60 * 60;
            double minutes = Convert.ToDouble(time.Substring(3, 2)) * 60;
            double seconds = Convert.ToDouble(time.Substring(6, 2));

            return hours + minutes + seconds;
        }
    }
}
