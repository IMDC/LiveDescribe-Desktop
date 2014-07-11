using LiveDescribe.Events;
using LiveDescribe.Model;
using NAudio;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LiveDescribe.Utilities
{
    public class DescriptionRecorder : INotifyPropertyChanged
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constants
        public const int DefaultDeviceNumber = 0;
        #endregion

        #region Fields
        private bool _isRecording;
        private bool _recordExtended;
        private int _deviceNumber;
        private double _descriptionStartTime;
        private ProjectFile _recordedFile;
        private WaveIn _microphonestream;
        private WaveFileWriter _waveWriter;
        /// <summary>
        /// Controls access to the recording methods, so that IsRecording accurately reflects
        /// whether or not the Recorder is recording.
        /// </summary>
        private object _recordingAccessLock = new object();
        #endregion

        #region Events
        public event EventHandler<EventArgs<Description>> DescriptionRecorded;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        public DescriptionRecorder()
        {
            _isRecording = false;
            _deviceNumber = DefaultDeviceNumber;

            if (Properties.Settings.Default.Microphone != null)
                MicrophoneDeviceNumber = Properties.Settings.Default.Microphone.DeviceNumber;
        }
        #endregion

        #region Properties
        public bool IsRecording
        {
            set
            {
                lock (_recordingAccessLock)
                {
                    _isRecording = value;
                    NotifyPropertyChanged();
                }
            }
            get
            {
                lock (_recordingAccessLock) { return _isRecording; }
            }
        }

        /// <summary>
        /// The microphone device number to use. NAudio pics mics by device number.
        /// </summary>
        public int MicrophoneDeviceNumber
        {
            set
            {
                _deviceNumber = value;
                NotifyPropertyChanged();
            }
            get { return _deviceNumber; }
        }

        /// <summary>
        /// The stream that the recorder reads data from. This property is private because it gets
        /// re-generated every time a description is recorded.
        /// </summary>
        private WaveIn MicrophoneStream
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

                //Setup
                if (_microphonestream != null)
                    _microphonestream.DataAvailable += MicrophoneSteam_DataAvailable;

                NotifyPropertyChanged();
            }
            get { return _microphonestream; }
        }
        #endregion

        #region Methods

        public bool CanRecord()
        {
            lock (_recordingAccessLock)
            {
                return true; //!IsRecording
            }
        }

        public void RecordDescription(ProjectFile file, bool recordExtended, double videoPositionMilliseconds)
        {
            lock (_recordingAccessLock)
            {
                Log.Info("Beginning to record audio");

                try
                {
                    MicrophoneStream = GetMicrophone(_deviceNumber);
                }
                catch (MmException)
                {
                    Log.Warn("Microphone not found");
                    throw;
                }
                Log.Info("Recording...");

                _waveWriter = new WaveFileWriter(file.AbsolutePath, MicrophoneStream.WaveFormat);
                _recordExtended = recordExtended;
                _recordedFile = file;

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
        }

        /// <summary>
        /// Creates a WaveIn object representing the microphone.
        /// </summary>
        /// <param name="deviceNumber">Device number to create microphone from.</param>
        /// <returns>Microphone WaveIn object.</returns>
        private WaveIn GetMicrophone(int deviceNumber)
        {
            var micStream = new WaveIn
            {
                DeviceNumber = deviceNumber,
                WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels)
            };
            Log.Info("Name of Microphone to Use: " + WaveIn.GetCapabilities(deviceNumber).ProductName);

            return micStream;
        }

        public bool MicrophoneAvailable()
        {
            try
            {
                WaveIn.GetCapabilities(_deviceNumber);
            }
            catch (MmException) { return false; }
            return true;
        }

        public void StopRecording()
        {
            lock (_recordingAccessLock)
            {
                Log.Info("Finished Recording");
                MicrophoneStream.StopRecording();
                _waveWriter.Dispose();
                _waveWriter = null;
                var read = new WaveFileReader(_recordedFile);

                var d = new Description(_recordedFile, 0, read.TotalTime.TotalMilliseconds,
                    _descriptionStartTime, _recordExtended);
                OnDescriptionRecorded(d);

                read.Dispose();

                IsRecording = false;
            }
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
            if (_waveWriter == null)
                return;

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
