using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace LiveDescribe.Utilities
{
    class AudioUtility
    {
        private struct Header
        {
            public byte[] chunkID; 
            public uint chunkSize; 
            public byte[] fmt; 
            public byte[] subChunk1ID; 
            public uint subChunk1Size;
            public ushort audioFormat;
            public ushort numChannels;
            public uint sampleRate;
            public uint byteRate; 
            public ushort blockAlign; 
            public ushort bitsPerSample;   
            public byte[] subChunk2ID;
            public uint subChunk2Size;
        }

        private string _videoFile;
        private string _audioFile;
        private Header _header = new Header();
 
        public AudioUtility(string videoFile)
        {
            _videoFile = videoFile;
            _audioFile = Path.GetFileNameWithoutExtension(_videoFile) + ".wav";
        }


        /// <summary>
        /// Strips the audio from the given video file and outputs a wave file using FFMPEG.
        /// </summary>
        /// <remarks>
        /// This function uses the executable FFMPEG file to handle the audio stripping
        /// </remarks>
        /// <param name="reportProgressWorker">Used to determine what the current progress of stripping the audio is</param>
        public void stripAudio(BackgroundWorker reportProgressWorker)
        {
            //gets the path of the ffmpeg.exe file within the LiveDescribe solution
            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string ffmpegPath = Path.Combine(appDirectory, "Utilities/ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                MessageBox.Show("Cannot find ffmpeg.exe" + " " + ffmpegPath);
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

            string text = null;
            double totalTime = 0;
            double currentTime = 0;
            //stream reader used to parse the output of the ffmpeg process
            StreamReader input = ffmpeg.StandardError;


            /* Parsing the output of ffmpeg to obtain the total time and the current time 
             to  calculate a percentage whose value is used to update the progress bar*/
            try
            {
                while (!input.EndOfStream)
                {
                    text = input.ReadLine();
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
                            Console.WriteLine("Percentage: " + percentComplete);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            ffmpeg.WaitForExit();

            var fileInfo = new FileInfo(_audioFile);
            Console.WriteLine("Audio file length: " + fileInfo.Length);
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
        public List<float> readWavData(BackgroundWorker reportProgressWorker)
        {
            double min = 0; //default to 0
            double max = 0; //default to 0
            List<float> data = new List<float>();

            using (FileStream fs = new FileStream(_audioFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                try
                {
                    //RIFF chunk
                    _header.chunkID = br.ReadBytes(4);
                    _header.chunkSize = br.ReadUInt32();
                    _header.fmt = br.ReadBytes(4);

                    //fmt sub chunk
                    _header.subChunk1ID = br.ReadBytes(4);
                    _header.subChunk1Size = br.ReadUInt32();
                    _header.audioFormat = br.ReadUInt16();
                    _header.numChannels = br.ReadUInt16();
                    _header.sampleRate = br.ReadUInt32();
                    _header.byteRate = br.ReadUInt32();
                    _header.blockAlign = br.ReadUInt16();
                    _header.bitsPerSample = br.ReadUInt16();

                    br.ReadBytes(2); //skip two bytes, this shouldn't need to happen

                    //data sub chunk
                    _header.subChunk2ID = br.ReadBytes(4);
                    _header.subChunk2Size = br.ReadUInt32();

                    double maxSampleValue = Math.Pow(2, _header.bitsPerSample);
                    int ratio = _header.numChannels == 2 ? 40 : 80;

                    //add the sample values to the data list
                    for (int dataPoint = 0; dataPoint < _header.subChunk2Size / _header.blockAlign; dataPoint++)
                    {
                        byte[] buffer = br.ReadBytes(2);
                        
                        if (dataPoint % ratio == 0)
                        {
                            double val = ((BitConverter.ToInt16(buffer, 0) + (maxSampleValue / 2)) / maxSampleValue);
                            data.Add((float)val);
                            buffer = null;

                            //finding min and max values for normalization method
                            if (dataPoint == 0)
                            {
                                min = val;
                                max = val;
                            }
                            else
                            {
                                min = val < min ? val : min;
                                max = val > max ? val : max;
                            }
                        }

                        if (_header.numChannels == 2)
                        {
                            //skip next sample 
                            br.ReadBytes(2);
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (br != null)
                    {
                        br.Close();
                    }
                    if (fs != null)
                    {
                        fs.Close();
                    }

                    data = normalizeData(data, (float)0.9, (float)0.1, (float)min, (float)max);
                }
            }

            #region Debug Info
            //Display the Contents of _header
            Console.WriteLine("ChunkID: " + Encoding.Default.GetString(_header.chunkID));
            Console.WriteLine("Chunksize: " + _header.chunkSize);
            Console.WriteLine("fmt: " + Encoding.Default.GetString(_header.fmt));
            Console.WriteLine("subChunk1ID: " + Encoding.Default.GetString(_header.subChunk1ID));
            Console.WriteLine("subChunk1Size: " + _header.subChunk1Size);
            Console.WriteLine("audioFormat: " + _header.audioFormat);
            Console.WriteLine("numChannels: " + _header.numChannels);
            Console.WriteLine("sampleRate: " + _header.sampleRate);
            Console.WriteLine("byteRate: " + _header.byteRate);
            Console.WriteLine("blockAlign: " + _header.blockAlign);
            Console.WriteLine("bitsPerSample: " + _header.bitsPerSample);
            Console.WriteLine("subChunk2Id: " + Encoding.Default.GetString(_header.subChunk2ID));
            Console.WriteLine("subChunk2Size: " + _header.subChunk2Size);
            Console.WriteLine("audioData Length: " + data.Count);
            #endregion

            return data;
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
        private List<float> normalizeData(List<float> data, float newMin, float newMax, float oldMin, float oldMax)
        {
            for (int dataPoint = 0; dataPoint < data.Count; dataPoint++)
            {
                float multFactor = (newMax - newMin) / (oldMax - oldMin);
                data[dataPoint] = (float)(((data[dataPoint] - oldMin) * multFactor) + newMin);
            }
            return data;
        }

    }
}
