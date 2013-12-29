﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework;
using Microsoft.TeamFoundation.MVVM;
using LiveDescribe.Interfaces;
using NAudio.Wave;
using LiveDescribe.Model;
using LiveDescribe.Events;

namespace LiveDescribe.View_Model
{
    public class DescriptionViewModel : ViewModelBase
    {

        #region Instance Variables
        private ObservableCollection<Description> _descriptions;
        private NAudio.Wave.WaveIn _microphonestream;
        private NAudio.Wave.WaveFileWriter waveWriter;
        private readonly ILiveDescribePlayer _mediaVideo;
        private bool _usingExistingMicrophone;
        private double _descriptionStartTime;

        //used to restore the previous video state after it's finished recording
        private LiveDescribeVideoStates _previousVideoState;
        #endregion

        #region Event Handlers
        public EventHandler<DescriptionEventArgs> AddDescriptionEvent;
        public EventHandler RecordRequested;
        public EventHandler RecordRequestedMicrophoneNotPluggedIn;
        #endregion

        #region Constructors

        public DescriptionViewModel(ILiveDescribePlayer mediaVideo)
        {
            waveWriter = null;
            RecordCommand = new RelayCommand(Record, RecordStateCheck);
            _mediaVideo = mediaVideo;

            //check if the current microphone exists in the settings
            //if it doesn't use the first one or else get the one stored in the settings
            if (Properties.Settings.Default.Microphone == null)
            {
                _usingExistingMicrophone = false;
            }
            else
            {
                _usingExistingMicrophone = true;
                MicrophoneStream = Properties.Settings.Default.Microphone;
            }
            Descriptions = new ObservableCollection<Description>();
        }
        #endregion

        #region Commands

        public RelayCommand RecordCommand
        {
            private set;
            get;
        }
        #endregion

        #region Binding Functions

        /// <summary>
        /// Records the description
        /// </summary>
        /// <param name="param"></param>
        public void Record(object param)
        {
            Console.WriteLine("----------------------");

            //if the button was clicked once already it is in the RecordingDescription State
            //so end the recording because it is the second click
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription)
            {    
                Console.WriteLine("Finished Recording");
                MicrophoneStream.StopRecording();
                string filename = waveWriter.Filename;
                waveWriter.Dispose();
                waveWriter = null;
                NAudio.Wave.WaveFileReader read = new NAudio.Wave.WaveFileReader(filename);
                AddDescription(filename, 0, read.TotalTime.TotalMilliseconds, _descriptionStartTime);
                read.Dispose();
                //have to change the state of recording
                _mediaVideo.CurrentState = _previousVideoState;
                return;
            }
            
            EventHandler handlerRecordRequested = RecordRequested;
            EventHandler handlerNotPluggedIn = RecordRequestedMicrophoneNotPluggedIn;
            
            // if we don't have an existing microphone we try to create a new one with the first available microphone
            // if no microphone exists an exception is thrown and we throw the event "RecordRequestedMicrophoneNotPluggedIn
            if (!_usingExistingMicrophone)
            {
                try
                {
                    MicrophoneStream = new NAudio.Wave.WaveIn
                    {
                        DeviceNumber = 0,
                        WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(0).Channels)
                    };
                    Console.WriteLine("Product Name of Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(0).ProductName);
                }
                catch (NAudio.MmException e)
                {
                    Console.WriteLine("Creating new microphone Exception....");
                    Console.WriteLine(e.StackTrace);

                    if (handlerNotPluggedIn == null) return;
                    RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);
                    return;
                }
            }
            Console.WriteLine("Recording..");
           
            // get a random guid to name the wave file
            // there is an EXTREMELY small chance that the guid used has been used before
            Guid g = Guid.NewGuid();
            waveWriter = new NAudio.Wave.WaveFileWriter(Properties.Settings.Default.WorkingDirectory + g.ToString() + ".wav", MicrophoneStream.WaveFormat);
            
            MicrophoneStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(MicrophoneSteam_DataAvailable);
  
            try
            {
                _descriptionStartTime = _mediaVideo.CurrentPosition.TotalMilliseconds;
                MicrophoneStream.StartRecording();
            }
            catch (NAudio.MmException e)
            {
                Console.WriteLine("Previous Microphone Exception...");
                Console.WriteLine(e.StackTrace);

                if (handlerNotPluggedIn == null) return;
                RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);
                return;
            }

            _previousVideoState = _mediaVideo.CurrentState;
            if (_mediaVideo != null) _mediaVideo.CurrentState = LiveDescribeVideoStates.RecordingDescription;
            if (handlerRecordRequested == null) return;
            handlerRecordRequested(this, EventArgs.Empty);
        }
  
        #endregion

        #region BindingProperties
        /// <summary>
        /// property to set the Microphonestream
        /// </summary>
        public NAudio.Wave.WaveIn MicrophoneStream
        {
            set
            {
                _microphonestream = value;
                RaisePropertyChanged("MicrophoneStream");
            }
            get
            {
                return _microphonestream;
            }
        }

        /// <summary>
        /// Property to set and get the ObservableCollection containing descriptions
        /// </summary>
        public ObservableCollection<Description> Descriptions
        {
            set
            {
                _descriptions = value;
                RaisePropertyChanged("Descriptions");
            }
            get
            {
                return _descriptions;
            }
        }
        #endregion

        #region State Checks

        public bool RecordStateCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return false;
            return true;
        }

        #endregion

        #region Private Event Methods

        private void MicrophoneSteam_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null) return;

            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        #endregion

        #region Helper Methods
        public void AddDescription(string filename, double startwavefiletime, double endwavefiletime, double startinvideo)
        {
            Description desc = new Description(filename, startwavefiletime, endwavefiletime, startinvideo);
            Descriptions.Add(desc);
            EventHandler<DescriptionEventArgs> addDescriptionHandler = AddDescriptionEvent;
            if (addDescriptionHandler == null) return;
            addDescriptionHandler(this, new DescriptionEventArgs(desc));
        }
        #endregion
    }
}
