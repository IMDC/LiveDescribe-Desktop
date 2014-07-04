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
        /// 
        /// </summary>
        public void exportVideoWithDescriptions()
        {
            createDescriptionTrack();
            //createblankaudio(33);
            //foreach (var description in _descriptionlist)
            //{
            //    console.writeline("{0}", description.audiofile);
            //    console.writeline("{0}", appendsilence(description.audiofile, 60));
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        private void createDescriptionTrack()
        {
            Log.Info("Preparing to Create Description Audio");
            List<string> concat_list = new List<string>();
            List<Description> descriptions = new List<Description>(_descriptionList);
            
            descriptions.Sort((x, y) => x.StartInVideo.CompareTo(y.StartInVideo));
            string init_silence = createBlankAudio(descriptions[0].StartInVideo); //create silence track for the begining of the description track

            for ( int i = 0; i < descriptions.Count; i++ )
            {
                double delta;

                if (i != descriptions.Count - 1) //not the last item
                {
                    delta = descriptions[i + 1].StartInVideo - descriptions[i].StartInVideo;
                }
                else
                {
                    delta = _videoDurationSeconds - descriptions[i].StartInVideo;
                }
                concat_list.Add(appendSilence(descriptions[i].AudioFile, delta));
            }

            string command = "";

            foreach (String file in concat_list)
            {
                command +=  " -i \"" + file + "\"";
            }

            command += " -filter_complex concat=n=" + concat_list.Count + ":v=0:a=1 " + "\""
                        + _project.Folders.Project + "\\descriptions\\combined_description_track.wav\"";
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

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="audioFile"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private string appendSilence(string audioFile, double duration)
        {
            string[] separator = new string[] {"\\"};
            string[] filePathSplit;
            string outFileName;

            //rename the file to be padded with silence
            filePathSplit = audioFile.Split(separator, StringSplitOptions.None);
            filePathSplit[filePathSplit.Length - 1] = "export_" + filePathSplit[filePathSplit.Length - 1];
            outFileName = String.Join("\\", filePathSplit);


            string command = " -i \"" + audioFile + "\" -filter_complex aevalsrc=0::d=" + duration 
                            + "[silence];[0:a][silence]concat=n=2:v=0:a=1[out] -map [out] \"" + outFileName +"\"";
            ffmpegCommand(command);

            return outFileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
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
