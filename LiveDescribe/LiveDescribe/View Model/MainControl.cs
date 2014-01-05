using LiveDescribe.Interfaces;
using Microsoft.TeamFoundation.MVVM;
using System.ComponentModel;
using System;

namespace LiveDescribe.View_Model
{
    class MainControl : ViewModelBase
    {
        #region Instance Variables
        private VideoControl _videocontrol;
        private PreferencesViewModel _preferences;
        private DescriptionViewModel _descriptionviewmodel;
        #endregion

        #region Events
        public EventHandler PlayRequested;
        public EventHandler PauseRequested;
        public EventHandler MuteRequested;
        #endregion

        #region Constructors
        public MainControl(ILiveDescribePlayer mediaVideo)
        {
            _videocontrol = new VideoControl(mediaVideo);
            _preferences = new PreferencesViewModel();
            _descriptionviewmodel = new DescriptionViewModel(mediaVideo);
            //If apply requested happens  in the preferences use the new saved microphone in the settings
            _preferences.ApplyRequested += (sender, e) =>
                {
                    _descriptionviewmodel.MicrophoneStream = Properties.Settings.Default.Microphone;
                    Console.WriteLine("Product Name of Apply Requested Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(_descriptionviewmodel.MicrophoneStream.DeviceNumber).ProductName);
                };

            _videocontrol.PlayRequested += (sender, e) =>
                {
                    mediaVideo.Play();

                    //this Handler should be attached to in the view to update the graphics
                    EventHandler handler = this.PlayRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.PauseRequested += (sender, e) =>
                {
                    mediaVideo.Pause();

                    //this Handler should be attached to in the view to update the graphics
                    EventHandler handler = this.PauseRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.MuteRequested += (sender, e) =>
                {
                    //this Handler should be attached to in the view to update the graphics
                    EventHandler handler = this.MuteRequested;
                    if (handler != null) handler(sender, e);
                };
        }
        #endregion

        #region Binding Functions

        #endregion

        #region Commands
        #endregion

        #region Binding Properties
        /// <summary>
        /// returns the video control so it can be binded to a control in the mainwindow
        /// </summary>
        public VideoControl VideoControl
        {
            get
            {
                return _videocontrol;
            }
        }

        /// <summary>
        /// returns the PreferenceViewModel so it can be binded to a control in the main window
        /// </summary>
        public PreferencesViewModel PreferencesViewModel
        {
            get
            {
                return _preferences;
            }
        }

        /// <summary>
        /// returns the description view model so it can be binded to a control in the main window
        /// </summary>
        public DescriptionViewModel DescriptionViewModel
        {
            get
            {
                return _descriptionviewmodel;
            }
        }
        #endregion

    }
}
