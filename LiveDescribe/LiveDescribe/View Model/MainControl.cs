using LiveDescribe.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using System;
using LiveDescribe.Model;
using System.Timers;

namespace LiveDescribe.View_Model
{
    class MainControl : ViewModelBase
    {
        #region Instance Variables
        private Timer _descriptiontimer;
        private VideoControl _videocontrol;
        private PreferencesViewModel _preferences;
        private DescriptionViewModel _descriptionviewmodel;
        private ILiveDescribePlayer _mediaVideo;
        #endregion

        #region Events
        public event EventHandler ProjectClosed;
        public event EventHandler GraphicsTick;
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler MediaEnded;
        #endregion

        #region Constructors
        public MainControl(ILiveDescribePlayer mediaVideo)
        {
            DispatcherHelper.Initialize();
            _videocontrol = new VideoControl(mediaVideo);
            _preferences = new PreferencesViewModel();
            _descriptionviewmodel = new DescriptionViewModel(mediaVideo);

            CloseProjectCommand = new RelayCommand(CloseProject, ()=>true);

            _mediaVideo = mediaVideo;
                      
            //If apply requested happens  in the preferences use the new saved microphone in the settings
            _descriptiontimer = new Timer(10);
            _descriptiontimer.Elapsed += (sender, e) => Play_Tick(sender, e);
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
        /// <summary>
        /// This function gets called when the close project menu item gets pressed
        /// </summary>
        /// <param name="param"></param>
        public void CloseProject()
        {
            //TODO: ask to save here before closing everything
            //TODO: put it in a background worker and create a loading screen (possibly a general use control)
            Console.WriteLine("Closed Project");
            _descriptionviewmodel.CloseDescriptionViewModel();
            _videocontrol.CloseVideoControl();
            EventHandler handler = ProjectClosed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion

        #region Commands
        public RelayCommand CloseProjectCommand { private set; get; }
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
                DispatcherHelper.UIDispatcher.Invoke(delegate { current = _mediaVideo.CurrentPosition; });
                Console.WriteLine(current.TotalMilliseconds);
                double offset = current.TotalMilliseconds - curDescription.StartInVideo;
              
              //  Console.WriteLine("Offset: " + offset);
                if (!curDescription.IsExtendedDescription && 
                    offset >= 0 && offset < (curDescription.EndWaveFileTime - curDescription.StartWaveFileTime))
                {
                 //   Console.WriteLine("Playing Regular Description");
                    curDescription.Play(offset);
                    break;
                }
                else if (curDescription.IsExtendedDescription &&
                    offset < 100 && offset >= -100)
                {

                    //this method technically runs on the UI thread because it is an event that gets called in the description class
                    //therefore does not need to be invoked by the Dispatcher
                    curDescription.DescriptionFinishedPlaying += (sender1, e1) =>
                    {
                      //  _mediaVideo.CurrentPosition = new TimeSpan((int)_mediaVideo.CurrentPosition.TotalDays,(int) _mediaVideo.CurrentPosition.TotalHours, 
                      //       (int)_mediaVideo.CurrentPosition.TotalMinutes, (int)_mediaVideo.CurrentPosition.TotalSeconds, (int)_mediaVideo.CurrentPosition.TotalMilliseconds + 100);
                        _videocontrol.PlayCommand.Execute(this);
                    };

                    //Invoke the commands on the UI thread
                    DispatcherHelper.UIDispatcher.Invoke(delegate { _videocontrol.PauseCommand.Execute(this); curDescription.Play(); });
                }
            }
        }
        #endregion
    }
}
