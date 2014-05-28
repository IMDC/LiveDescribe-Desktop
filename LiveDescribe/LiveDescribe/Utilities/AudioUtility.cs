using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using LiveDescribe.Model;
using log4net.Repository.Hierarchy;
using NAudio.Wave;
using System.Linq;

namespace LiveDescribe.Utilities
{
    public class AudioUtility
    {
        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private readonly string _videoFile;
        private readonly string _audioFile;
        private Header _header;
 
        public AudioUtility(Project p)
        {
            _videoFile = p.Files.Video;
            _audioFile = Path.Combine(p.Folders.Cache, Path.GetFileNameWithoutExtension(_videoFile) + ".wav");
            _header = new Header();
        }

        #region Getter / Setter
        public Header Header
        {
            get { return this._header; }
        }
        #endregion

        /// <summary>
        /// Strips the audio from the given video file and outputs a wave file using FFMPEG.
        /// </summary>
        /// <remarks>
        /// This function uses the executable FFMPEG file to handle the audio stripping
        /// </remarks>
        /// <param name="reportProgressWorker">Used to determine what the current progress of stripping the audio is</param>
        public void StripAudio(BackgroundWorker reportProgressWorker)
        {
            log.Info("Preparing to strip audio from video");
            //gets the path of the ffmpeg.exe file within the LiveDescribe solution
            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string ffmpegPath = Path.Combine(appDirectory, "Utilities/ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                MessageBox.Show("Cannot find ffmpeg.exe " + ffmpegPath);
                log.Error("ffmpeg.exe can not be found at " + ffmpegPath);
                return;
            }

            //the parameters that are sent to the ffmpeg command
            string strParam = " -i \"" + _videoFile + "\" -ac 2 -ab 160k -ar 44100 -f wav -vn -y \"" + _audioFile + "\"";

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

            double totalTime = 0;
            double currentTime = 0;
            //stream reader used to parse the output of the ffmpeg process
            StreamReader input = ffmpeg.StandardError;

            log.Info("Attempting to parse ffmpeg output");
            /* Parsing the output of ffmpeg to obtain the total time and the current time 
             to  calculate a percentage whose value is used to update the progress bar*/
            try
            {
                while (!input.EndOfStream)
                {
                    string text = input.ReadLine();
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

                                totalTime = getTime(time);
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

                                currentTime = getTime(time);
                            }
                        }
                    }

                    //updates the progress bar given that the total time is not zero
                    if (totalTime != 0)
                    {
                        int percentComplete = Convert.ToInt32(((double)currentTime / (double)totalTime) * 100);
                        if (percentComplete <= 100)
                        {
                            reportProgressWorker.ReportProgress(percentComplete);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                log.Error("An error occured during ffmpeg audio stripping",ex);
            }

            ffmpeg.WaitForExit();

            var fileInfo = new FileInfo(_audioFile);
            log.Info("Audio file length: " + fileInfo.Length);
        }

        /// <summary>
        /// Convert from ffmpeg time to seconds 
        /// </summary>
        /// <param name="time">the time in ffmpeg format HH:MM:SS</param>
        /// <returns></returns>
        private double getTime(string time)
        {
            double hours = Convert.ToDouble(time.Substring(0, 2)) * 60 * 60;
            double minutes = Convert.ToDouble(time.Substring(3, 2)) * 60;
            double seconds = Convert.ToDouble(time.Substring(6, 2));

            return hours + minutes + seconds;
        }


        /// <summary>
        /// Read the wav data from the stripped audio and return the sample data
        /// </summary>
        /// <param name="reportProgressWorker">Used to determine what the current progress of stripping the audio is</param>
        /// <returns>data</returns>
        public List<short> ReadWavData(BackgroundWorker reportProgressWorker)
        {
            var data = new List<short>();

            log.Info("Opening .wav file for reading at " + _audioFile);
            using (var fs = new FileStream(_audioFile, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                log.Info("Reading header info from .wav file");
                //RIFF chunk
                _header.ChunkId = br.ReadBytes(4);
                _header.ChunkSize = br.ReadUInt32();
                _header.Fmt = br.ReadBytes(4);

                //fmt sub chunk
                _header.SubChunk1Id = br.ReadBytes(4);
                _header.SubChunk1Size = br.ReadUInt32();
                _header.AudioFormat = br.ReadUInt16();
                _header.NumChannels = br.ReadUInt16();
                _header.SampleRate = br.ReadUInt32();
                _header.ByteRate = br.ReadUInt32();
                _header.BlockAlign = br.ReadUInt16();
                _header.BitsPerSample = br.ReadUInt16();

                br.ReadBytes(2); //skip two bytes, this shouldn't need to happen

                //data sub chunk
                _header.SubChunk2Id = br.ReadBytes(4);
                _header.SubChunk2Size = br.ReadUInt32();

                double maxSampleValue = Math.Pow(2, _header.BitsPerSample);
                int ratio = _header.NumChannels == 2 ? 40 : 80;

                log.Info("Reading sound data from .wav file");
                for (int i = 0; i < _header.SubChunk2Size; i+=ratio)
                {
                    data.Add(br.ReadInt16());
                    //Skip the next n-2 bytes
                    br.ReadBytes(ratio - 2);
                }

                log.Info("Sound data successfully read in from .wav file");
                return data;
            }
        }


        /// <summary>
        /// Will normalized the data using a linear transformation
        /// using the following formula:
        ///     I(n) = [(I(n) - min) newMax - newMin / max - min] + newMin
        /// </summary>
        /// <param name="data">data</param>
        /// <param name="newMin">newMin</param>
        /// <param name="newMax">newMax</param>
        /// <param name="oldMin">oldMin</param>
        /// <param name="oldMax">oldMax</param>
        /// <returns></returns>
        private List<short> normalizeData(List<short> data, float newMin, float newMax, float oldMin, float oldMax)
        {
            for (int dataPoint = 0; dataPoint < data.Count; dataPoint++)
            {
                float multFactor = (newMax - newMin) / (oldMax - oldMin);
                data[dataPoint] = (short)(((data[dataPoint] - oldMin) * multFactor) + newMin);
            }
            return data;
        }

        /// <summary>
        /// Calculates the Root Mean Square of the audio data
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>root mean square</returns>
        private float RMS(List<short> data)
        {
            float total = 0;
            float mid = 0.5F;
            for (int i = 0; i < data.Count; i++)
            {
                total += (float)Math.Pow(data[i] - mid, 2);
            }

            return (float)Math.Sqrt(total / data.Count);
        }


        /// <summary>
        /// Calculates the energy of the audio data
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>energy</returns>
        private float Energy(List<short> data)
        {
            float total = 0;
            float mid = 0.5F;
            for (int i = 0; i < data.Count; i++)
            {
                total += (float)Math.Pow(data[i] - mid, 2);
            }

            return total;
        }

        /// <summary>
        /// Finds the Zero-crossing rate for the List "data"
        /// passed to it
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>crosses</returns>
        private float ZCR(List<short> data)
        {
            float crosses = 0;
            float prev;
            float current;
            float mid = 0.5F;

            for (int i = 1; i < data.Count; i++)
            {
                current = data[i] - mid;
                prev = data[i - 1] - mid;
                if ( (current * prev) < 0)
                {
                    crosses++;
                }
            }
            return crosses;
        }


        /// <summary>
        /// Finds the non-speech regions in audio data
        /// based on zero-crossing rate and energy descriptors
        /// </summary>
        /// <param name="data"></param>
        /// <returns>spaces</returns>
        public List<Space> findSpaces(List<short> data)
        { 
            List<Space> spaces = new List<Space>();
            float duration = (float)(_header.ChunkSize * 8 ) / ( _header.SampleRate * _header.BitsPerSample * _header.NumChannels);
            int ratio = _header.NumChannels == 2 ? 40 : 80;
            double samples_per_second = _header.SampleRate * (_header.BlockAlign / (double)ratio);
            
            Console.WriteLine("Duration: {0} samples_per_second: {1}, sample_rate: {2}, BlockAlign: {3} Data Size; {4}", duration, samples_per_second, _header.SampleRate, _header.BlockAlign, data.Count);

            //keeps track of sounds descriptors for each window
            List<float> zcr_histogram = new List<float>();
            List<float> energy_histogram = new List<float>();

            int windowSize = 1; // 1 SECOND 
            int window = 0; //counter for the total number of windows

            List<short> bin; //section of the audio data
            
            List<int> result = new List<int>(); //hold whether or not the window contained non-speech or not (0 for no speech and 1 otherwise)

            //do through each window and calculate the audio descriptors
            while (window < duration)
            { 
                int start = (int)Math.Round(window * (windowSize * samples_per_second));
                
                if (start + samples_per_second < data.Count)
                { 
                    bin = data.GetRange(start, (int)samples_per_second);
                    zcr_histogram.Add(ZCR(bin));
                    energy_histogram.Add(Energy(bin));
                }
                window++;
            }

            double zcr_factor = 0.80;
            double energy_factor = 0.4;

            double zcr_avg = zcr_factor * zcr_histogram.Average();
            double energy_avg = energy_factor * energy_histogram.Average();

            //decide if a window should be marked as speech or not
            for (int i = 0; i < zcr_histogram.Count; i++)
            {
                if ((zcr_histogram[i] == 0) || (energy_histogram[i] == 0) || (zcr_histogram[i] > zcr_avg && energy_histogram[i] < energy_avg))
                {
                    //no speech
                    //check consecutive windows to find the end of this space
                    for (int j = i + 1; j < zcr_histogram.Count; j++) 
                    {
                        if (! ((zcr_histogram[j] == 0) || (energy_histogram[j] == 0) || (zcr_histogram[j] > zcr_avg && energy_histogram[j] < energy_avg)))
                        {
                            spaces.Add(new Space(i * 1000, j * 1000));
                            i = j;
                            break;
                        }
                    }
                }
                else
                {
                    continue; //speech
                }
            }
            return spaces;
        }

        /// <summary>
        /// Attempts to delete the file created by stripping the audio.
        /// </summary>
        public void DeleteAudioFile()
        {
            log.Info("Deleting audio .wav file");
            try
            {
                File.Delete(_audioFile);
            }
            catch (Exception e)
            {
                log.Warn("An error occured while trying to delete the audio file", e);
            }

            if (!File.Exists(_audioFile))
                log.Info("Audio .wav file deleted");
            else
                log.Info("Audio .wav file NOT deleted");
        }
    }
}
