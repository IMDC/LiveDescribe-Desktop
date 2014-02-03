using LiveDescribe.Interfaces;
using Microsoft.TeamFoundation.MVVM;
using System.ComponentModel;
using System;
using LiveDescribe.Model;
using System.Timers;

namespace LiveDescribe.View_Model
{
    class MainControl : ViewModelBase
    {
        #region Instance Variables
     //   private DispatcherTimer _descriptiontimer;
        private Timer _descriptiontimer;
        private VideoControl _videocontrol;
        private PreferencesViewModel _preferences;
        private DescriptionViewModel _descriptionviewmodel;
        private ILiveDescribePlayer _mediaVideo;
        #endregion

        #region Events
        public event EventHandler GraphicsTick;
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler MediaEnded;
        #endregion

        #region Constructors
        public MainControl(ILiveDescribePlayer mediaVideo)
        {
            _videocontrol = new VideoControl(mediaVideo);
            _preferences = new PreferencesViewModel();
            _descriptionviewmodel = new DescriptionViewModel(mediaVideo);

            _mediaVideo = mediaVideo;
                      
            //If apply requested happens  in the preferences use the new saved microphone in the settings
            _descriptiontimer = new Timer(10);
            _descriptiontimer.Elapsed += new ElapsedEventHandler(Play_Tick);
            _descriptiontimer.AutoReset = true;

            _preferences.ApplyRequested += (sender, e) =>
                {
                    _descriptionviewmodel.MicrophoneStream = Properties.Settings.Default.Microphone;
                    Console.WriteLine("Product Name of Apply Requested Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(_descriptionviewmodel.MicrophoneStream.DeviceNumber).ProductName);
                };

            _videocontrol.PlayRequested += (sender, e) =>
                {
                    mediaVideo.Play();
                    _descriptiontimer.Start();
                    //this Handler should be attached to the view to update the graphics
                    EventHandler handler = this.PlayRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.PauseRequested += (sender, e) =>
                {
                    mediaVideo.Pause();
                    _descriptiontimer.Stop();
                    //this Handler should be attached to the view to update the graphics
                    EventHandler handler = this.PauseRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.MuteRequested += (sender, e) =>
                {
                    
                    //this Handler should be attached to the view to update the graphics
                    mediaVideo.IsMuted = !mediaVideo.IsMuted;
                    EventHandler handler = this.MuteRequested;
                    if (handler != null) handler(sender, e);
                };

            _videocontrol.MediaEndedEvent += (sender, e) =>
                {
                    _descriptiontimer.Stop();
                    mediaVideo.Stop();
                    EventHandler handler = this.MediaEnded;
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

        #region Helper Functions
        /// <summary>
        /// Gets called by the description timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Tick(object sender, ElapsedEventArgs e)
        {

            EventHandler handler = GraphicsTick;
            if (handler != null) handler(sender, e);

            //I put this method in it's own timer in the MainControl for now, because I believe it should be separate from the view
            //this could possibly put it in the view in the other timer, so only one timer would be running
            for (int i = 0; i < _descriptionviewmodel.Descriptions.Count; ++i)
            {
                Description curDescription = _descriptionviewmodel.Descriptions[i];
                TimeSpan current = new TimeSpan();
                //get the current position of the video from the UI thread
                Dispatcher.Invoke(delegate { current = _mediaVideo.CurrentPosition; });
                double offset = current.TotalMilliseconds - curDescription.StartInVideo;
              
                Console.WriteLine("Offset: " + offset);
                if (!curDescription.IsExtendedDescription && 
                    offset >= 0 && offset < (curDescription.EndWaveFileTime - curDescription.StartWaveFileTime))
                {
                 //   Console.WriteLine("Playing Regular Description");
                    curDescription.Play(offset);
                    break;
                }

            }
        }
        #endregion
    }
}
