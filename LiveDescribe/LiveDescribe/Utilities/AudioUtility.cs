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
using NAudio.Wave;

namespace LiveDescribe.Utilities
{
    public class AudioUtility
    {
        private struct Header
        {
            public byte[] ChunkId; 
            public uint ChunkSize; 
            public byte[] Fmt; 
            public byte[] SubChunk1Id; 
            public uint SubChunk1Size;
            public ushort AudioFormat;
            public ushort NumChannels;
            public uint SampleRate;
            public uint ByteRate; 
            public ushort BlockAlign; 
            public ushort BitsPerSample;   
            public byte[] SubChunk2Id;
            public uint SubChunk2Size;
        }

        private readonly string _videoFile;
        private readonly string _audioFile;
        private Header _header;
 
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
        public void StripAudio(BackgroundWorker reportProgressWorker)
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
        public List<float> ReadWavData(BackgroundWorker reportProgressWorker)
        {
            double min = 0; //default to 0
            double max = 0; //default to 0
            var data = new List<float>();

            float[] buffer;
            using (var reader = new AudioFileReader(_audioFile))
            {
                var samples = reader.Length / (reader.WaveFormat.BitsPerSample / 8);

                if (reader.WaveFormat.Channels == 1)
                {
                    buffer = new float[samples];
                    reader.Read(buffer, 0, (int)samples);
                }
                else if (reader.WaveFormat.Channels == 2)
                {
                    int ratio = reader.WaveFormat.Channels == 2 ? 40 : 80;

                    buffer = new float[ratio];
                    List<float> wavData = new List<float>();
                    for (int i = 0; i < reader.Length; i+=buffer.Length)
                    {
                        //Read 2 bytes from Left Channel only
                        reader.Read(buffer, 0, 2);
                        wavData.Add(buffer[0]);
                        wavData.Add(buffer[1]);

                        //Advance the reader by n-2 bytes
                        reader.Read(buffer, 0, buffer.Length-2);
                    }
                    return wavData;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return new List<float>(buffer);

            /*
            using (var fs = new FileStream(_audioFile, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                try
                {
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

                    //add the sample values to the data list
                    for (int dataPoint = 0; dataPoint < _header.SubChunk2Size / _header.BlockAlign; dataPoint++)
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

                        if (_header.NumChannels == 2)
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
             */

            #region Debug Info
            //Display the Contents of _header
            Console.WriteLine("ChunkID: " + Encoding.Default.GetString(_header.ChunkId));
            Console.WriteLine("Chunksize: " + _header.ChunkSize);
            Console.WriteLine("fmt: " + Encoding.Default.GetString(_header.Fmt));
            Console.WriteLine("subChunk1ID: " + Encoding.Default.GetString(_header.SubChunk1Id));
            Console.WriteLine("subChunk1Size: " + _header.SubChunk1Size);
            Console.WriteLine("audioFormat: " + _header.AudioFormat);
            Console.WriteLine("numChannels: " + _header.NumChannels);
            Console.WriteLine("sampleRate: " + _header.SampleRate);
            Console.WriteLine("byteRate: " + _header.ByteRate);
            Console.WriteLine("blockAlign: " + _header.BlockAlign);
            Console.WriteLine("bitsPerSample: " + _header.BitsPerSample);
            Console.WriteLine("subChunk2Id: " + Encoding.Default.GetString(_header.SubChunk2Id));
            Console.WriteLine("subChunk2Size: " + _header.SubChunk2Size);
            Console.WriteLine("audioData Length: " + data.Count);
            #endregion
            findSpaces(data);
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

        /// <summary>
        /// Calculates the Root Mean Square of the audio data
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>root mean square</returns>
        private float RMS(List<float> data)
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
        private float Energy(List<float> data)
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
        private float ZCR(List<float> data)
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
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<Space> findSpaces(List<float> data)
        { 
            float duration = (float)(_header.ChunkSize * 8 ) / ( _header.SampleRate * _header.BitsPerSample * _header.NumChannels);
            int windowSize = 100; //100 samples 
            int windows = 0; //counter for the total number of windows
            int benchmark = 30; //30 windows for getting base window
            List<float> bin; //section of the audio data
            float zcr; //zero-crossing rate
            float energy; //energy
            float zcr_thresh = 0; //threshold
            float energy_thresh = 0; //threshold
            List<int> result = new List<int>(); //hold whether or not the window contained non-speech or not (0 for no speech and 1 otherwise)

            if (windowSize * benchmark >= data.Count/2) //audio doesn't contain enough data to analyze
            {
                throw new Exception("Data size too small");
            }

            //read first few windows of data (assuming the first few contain no speech)
            for (int i = 0; i < benchmark; i++)
            { 
                bin = data.GetRange((int)(i * windowSize), windowSize);
                zcr_thresh += ZCR(bin);
                energy_thresh += Energy(bin);
            }

            //get averages for the thresholds
            zcr_thresh = zcr_thresh / benchmark;
            energy_thresh = energy_thresh / benchmark;
            float combined_thresh = zcr_thresh * energy_thresh;

          //  Console.WriteLine("zcr-thresh: " + zcr_thresh);
          //  Console.WriteLine("energy-thresh: " + energy_thresh);
         //   Console.WriteLine("combined-thresh: " + combined_thresh);
            //Thread.Sleep(10000);
            
            for (int dataPoint = 0; dataPoint * windowSize < data.Count - windowSize; dataPoint++)
            {
                bin = data.GetRange((int)(dataPoint * windowSize), windowSize);
                zcr = ZCR(bin);
                energy = Energy(bin);

                if (zcr * energy <= combined_thresh)
                {
                    result.Add(0);
                }
                else 
                {
                    result.Add(1);
                }
                
              //  Console.WriteLine("energy: " + energy);
              //  Console.WriteLine("zcr: " + zcr);
                windows++;
            }

            #region debugInfo
            Console.WriteLine("WindowSize: " + windowSize);
            Console.WriteLine("Duration: " + duration);
            Console.WriteLine("Windows: " + windows);
            #endregion

            return new List<Space>();
        }

        /// <summary>
        /// Attempts to delete the file created by stripping the audio.
        /// </summary>
        public void DeleteAudioFile()
        {
            try
            {
                File.Delete(_audioFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured attempting to delete the audio file: {0}", e.ToString());
            }

            if (!File.Exists(_audioFile))
                Console.WriteLine("File deleted");
            else
                Console.WriteLine("File not deleted");
        }
    }
}
