using System;
using System.Collections.Generic;
using System.Linq;
using LiveDescribe.Model;

namespace LiveDescribe.Utilities
{
    /// <summary>
    /// Analyzes the data in a given audio source.
    /// </summary>
    public static class AudioAnalyzer
    {
        /// <summary>
        /// Calculates the energy of the audio data
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>energy</returns>
        private static float Energy(List<short> data)
        {
            float total = 0;
            const float mid = 0.5F;
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
        private static float Zcr(List<short> data)
        {
            float crosses = 0;
            const float mid = 0.5F;

            for (int i = 1; i < data.Count; i++)
            {
                float current = data[i] - mid;
                float prev = data[i - 1] - mid;
                if ((current * prev) < 0)
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
        /// <param name="waveform">Wavefrom to get spaces from.</param>
        /// <returns>spaces</returns>
        public static List<Space> FindSpaces(Waveform waveform)
        {
            var spaces = new List<Space>();
            float duration = (float)(waveform.Header.ChunkSize * 8)
                / (waveform.Header.SampleRate * waveform.Header.BitsPerSample * waveform.Header.NumChannels);
            int ratio = waveform.Header.NumChannels == 2 ? 40 : 80;
            double samplesPerSecond = waveform.Header.SampleRate * (waveform.Header.BlockAlign / (double)ratio);

            //Console.WriteLine("Duration: {0} samples_per_second: {1}, sample_rate: {2}, BlockAlign: {3} Data Size; {4}",
            //    duration, samples_per_second, waveform.Header.SampleRate, waveform.Header.BlockAlign, waveform.Data.Count);

            //keeps track of sounds descriptors for each window
            var zcrHistogram = new List<float>();
            var energyHistogram = new List<float>();

            const int windowSize = 1; // 1 SECOND
            int window = 0; //counter for the total number of windows

            //do through each window and calculate the audio descriptors
            while (window < duration)
            {
                var start = (int)Math.Round(window * (windowSize * samplesPerSecond));

                if (start + samplesPerSecond < waveform.Data.Count)
                {
                    List<short> bin = waveform.Data.GetRange(start, (int)samplesPerSecond); //section of the audio waveform.Data
                    zcrHistogram.Add(Zcr(bin));
                    energyHistogram.Add(Energy(bin));
                }
                window++;
            }

            const double zcrFactor = 0.80;
            const double energyFactor = 0.4;

            double zcrAvg = zcrFactor * zcrHistogram.Average();
            double energyAvg = energyFactor * energyHistogram.Average();

            //decide if a window should be marked as speech or not
            for (int i = 0; i < zcrHistogram.Count; i++)
            {
                if ((zcrHistogram[i] == 0) || (energyHistogram[i] == 0) || (zcrHistogram[i] > zcrAvg && energyHistogram[i] < energyAvg))
                {
                    //no speech
                    //check consecutive windows to find the end of this space
                    for (int j = i + 1; j < zcrHistogram.Count; j++)
                    {
                        if (!((zcrHistogram[j] == 0) || (energyHistogram[j] == 0) || (zcrHistogram[j] > zcrAvg && energyHistogram[j] < energyAvg)))
                        {
                            spaces.Add(new Space(i * 1000, j * 1000));
                            i = j;
                            break;
                        }
                    }
                }
            }
            return spaces;
        }
    }
}
