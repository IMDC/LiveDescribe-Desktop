using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Events;
using LiveDescribe.Model;
using NAudio;
using NAudio.Wave;

namespace LiveDescribe.Utilities
{
    public class DescriptionRecorder : INotifyPropertyChanged
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private bool _isRecording;
        private readonly bool _useExistingMicrophone;
        private double _descriptionStartTime;
        private WaveIn _microphonestream;
        private WaveFileWriter _waveWriter;

        #region Events
        public event EventHandler<EventArgs<Description>> DescriptionRecorded;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public DescriptionRecorder()
        {
            _isRecording = false;

            if (Properties.Settings.Default.Microphone == null)
            {
                _useExistingMicrophone = false;
            }
            else
            {
                _useExistingMicrophone = true;
                MicrophoneStream = Properties.Settings.Default.Microphone;
            }
        }

        public bool IsRecording
        {
            set
            {
                _isRecording = value;
                NotifyPropertyChanged();
            }
            get { return _isRecording; }
        }

        public WaveIn MicrophoneStream
        {
            set
            {
                //Cleanup
                if (_microphonestream != null)
                {
                    _microphonestream.DataAvailable -= MicrophoneSteam_DataAvailable;
                    _microphonestream.Dispose();
                }

                _microphonestream = value;

                if (_microphonestream != null)
                    _microphonestream.DataAvailable += MicrophoneSteam_DataAvailable;

                NotifyPropertyChanged();
            }
            get { return _microphonestream; }
        }

        #region Methods

        public bool CanRecord()
        {
            return true; //!IsRecording
        }

        public void RecordDescription(string filePath, double videoPositionMilliseconds)
        {
            Log.Info("Beginning to record audio");
            // if we don't have an existing microphone we try to create a new one with the first
            // available microphone if no microphone exists an exception is thrown and we throw the
            // event "RecordRequestedMicrophoneNotPluggedIn
            if (!_useExistingMicrophone)
            {
                try
                {
                    MicrophoneStream = GetMicrophone();
                }
                catch (MmException)
                {
                    Log.Warn("Microphone not found");
                    throw;
                }
            }
            Log.Info("Recording...");

            string path = filePath;
            _waveWriter = new WaveFileWriter(path, MicrophoneStream.WaveFormat);

            try
            {
                _descriptionStartTime = videoPositionMilliseconds;
                MicrophoneStream.StartRecording();
            }
            catch (MmException)
            {
                Log.Error("Previous Microphone was found then unplugged (No Microphone) Exception...");
                throw;
            }

            IsRecording = true;
        }

        private WaveIn GetMicrophone()
        {
            var micStream = new WaveIn
            {
                DeviceNumber = 0,
                WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(0).Channels)
            };
            Log.Info("Product Name of Microphone: " + WaveIn.GetCapabilities(0).ProductName);

            return micStream;
        }

        //TODO: get rid of paramaters
        public void StopRecording(string projectFolderPath, bool createExtendedDescription)
        {
            Log.Info("Finished Recording");
            MicrophoneStream.StopRecording();
            string audioFilePath = _waveWriter.Filename;
            _waveWriter.Dispose();
            _waveWriter = null;
            var read = new WaveFileReader(audioFilePath);

            var file = ProjectFile.FromAbsolutePath(audioFilePath, projectFolderPath);

            var d = new Description(file, 0, read.TotalTime.TotalMilliseconds, _descriptionStartTime, createExtendedDescription);
            OnDescriptionRecorded(d);

            read.Dispose();

            IsRecording = false;
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Write to the wave file, on data available in the microphone stream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MicrophoneSteam_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waveWriter == null) return;

            _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveWriter.Flush();
        }
        #endregion

        #region Event Invokations
        private void OnDescriptionRecorded(Description d)
        {
            EventHandler<EventArgs<Description>> handler = DescriptionRecorded;
            if (handler != null) handler(this, new EventArgs<Description>(d));
        }

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
