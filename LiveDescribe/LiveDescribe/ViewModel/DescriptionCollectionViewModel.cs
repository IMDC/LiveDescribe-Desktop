using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Events;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using NAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LiveDescribe.ViewModel
{
    public class DescriptionCollectionViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private ObservableCollection<Description> _alldescriptions;      //this list contains all the descriptions both regular and extended
        private ObservableCollection<Description> _extendedDescriptions; //this list only contains the extended description this list should be used to bind to the list view of extended descriptions
        private ObservableCollectionIndexer<Description> _extendedDescriptionIndexer;
        private ObservableCollection<Description> _regularDescriptions;  //this list only contains all the regular descriptions this list should only be used to bind to the list of regular descriptions
        private ObservableCollectionIndexer<Description> _regularDescriptionIndexer;
        private readonly ILiveDescribePlayer _mediaVideo;
        private DescriptionRecorder _recorder;
        private RelayCommand _recordButtonClickCommand;

        private bool _recordingExtendedDescription;

        //this variable should be used as little as possible in this class
        //most interactions between the  descriptioncollectionviewmodel and the MediaControlViewModel should be in the MainWindowViewModel
        private readonly MediaControlViewModel _mediaControlViewModel;

        private LiveDescribeVideoStates _previousVideoState; //used to restore the previous video state after it's finished recording

        public Project Project { get; set; }
        #endregion

        #region Event Handlers
        public event EventHandler<DescriptionEventArgs> AddDescriptionEvent;
        #endregion

        #region Constructors
        public DescriptionCollectionViewModel(ILiveDescribePlayer mediaVideo,
            MediaControlViewModel mediaControlViewModel, ProjectManager projectManager)
        {
            _mediaVideo = mediaVideo;
            _mediaControlViewModel = mediaControlViewModel;

            Project = null;
            _recorder = GetDescriptionRecorder();

            RecordCommand = new RelayCommand(
                canExecute: () =>
                    Project != null
                    && _mediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded
                    && _recorder.CanRecord(),
                execute: () =>
                {
                    try
                    {
                        var pf = Project.GenerateDescriptionFile();
                        _recorder.RecordDescription(pf, ExtendedIsChecked, _mediaVideo.Position.TotalMilliseconds);
                        //save the current state so when the button is pressed again you can restore it back to that state
                        _previousVideoState = _mediaVideo.CurrentState;
                    }
                    catch (MmException e)
                    {
                        MessageBoxFactory.ShowError("No Microphone Connected");
                        Log.Warn("No Microphone Connected", e);
                    }
                    _mediaVideo.CurrentState = LiveDescribeVideoStates.RecordingDescription;
                    RecordButtonClickCommand = StopRecordingCommand;
                });

            StopRecordingCommand = new RelayCommand(
                canExecute: () =>
                    Project != null
                    && _mediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded
                    && _recorder.IsRecording,
                execute: () =>
                    {
                        _recorder.StopRecording();
                        _mediaVideo.CurrentState = _previousVideoState;
                        RecordButtonClickCommand = RecordCommand;
                    });

            AllDescriptions = new ObservableCollection<Description>();
            RegularDescriptions = new ObservableCollection<Description>();
            _regularDescriptionIndexer = new ObservableCollectionIndexer<Description>(RegularDescriptions);
            ExtendedDescriptions = new ObservableCollection<Description>();
            _extendedDescriptionIndexer = new ObservableCollectionIndexer<Description>(ExtendedDescriptions);

            projectManager.Descriptions = AllDescriptions;
            projectManager.DescriptionsLoaded += (sender, args) => AddDescriptions(args.Value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for RecordCommand gets bound to the record button
        /// </summary>
        private RelayCommand RecordCommand { get; set; }
        private RelayCommand StopRecordingCommand { get; set; }

        public RelayCommand RecordButtonClickCommand
        {
            get { return _recordButtonClickCommand ?? (_recordButtonClickCommand = RecordCommand); }
            set
            {
                _recordButtonClickCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Properties
        public DescriptionRecorder Recorder
        {
            set
            {
                _recorder = value;
                RaisePropertyChanged();
            }
            get { return _recorder; }
        }

        /// <summary>
        /// Property to set and get the ObservableCollection containing all of the descriptions
        /// </summary>
        public ObservableCollection<Description> AllDescriptions
        {
            set
            {
                _alldescriptions = value;
                RaisePropertyChanged();
            }
            get { return _alldescriptions; }
        }

        /// <summary>
        /// Property to set and get the collection with all the extended descriptions should be
        /// bound to the extended description list
        /// </summary>
        public ObservableCollection<Description> ExtendedDescriptions
        {
            set
            {
                _extendedDescriptions = value;
                RaisePropertyChanged();
            }
            get { return _extendedDescriptions; }
        }

        /// <summary>
        /// Property to set and get the collection with all the regular descriptions should be bound
        /// to the regular description list
        /// </summary>
        public ObservableCollection<Description> RegularDescriptions
        {
            set
            {
                _regularDescriptions = value;
                RaisePropertyChanged();
            }
            get { return _regularDescriptions; }
        }

        /// <summary>
        /// Property that gets set when the extended description checkbox is checked or unchecked
        /// </summary>
        public bool ExtendedIsChecked
        {
            set
            {
                _recordingExtendedDescription = value;
                RaisePropertyChanged();
            }
            get { return _recordingExtendedDescription; }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Method to add a description to the list and throw an event, whenever you are adding a
        /// description to the list you should use this method
        /// </summary>
        /// <param name="filename">Filename of the description</param>
        /// <param name="startwavefiletime">The start time in the wav file of the description</param>
        /// <param name="endwavefiletime">The end time in the wav file of the description</param>
        /// <param name="startinvideo">The time in the video the description should start playing</param>
        /// <param name="isExtendedDescription">Whether it is an extended description or not</param>
        public void AddDescription(ProjectFile filename, double startwavefiletime, double endwavefiletime,
            double startinvideo, bool isExtendedDescription)
        {
            AddDescription(new Description(filename, startwavefiletime, endwavefiletime, startinvideo,
                isExtendedDescription));
        }

        public void AddDescription(Description desc)
        {
#if ZAGGA
            if (desc.IsExtendedDescription)
                return;
#endif

            if (!desc.IsExtendedDescription)
                RegularDescriptions.Add(desc);
            else
                ExtendedDescriptions.Add(desc);

            SetupEventsOnDescription(desc);

            AllDescriptions.Add(desc);
            OnAddDescription(desc);
        }

        public void AddDescriptions(List<Description> descriptions)
        {
            foreach (var desc in descriptions)
                AddDescription(desc);
        }

        /// <summary>
        /// Method to setup events on a descriptions no graphics setup should be included in here,
        /// that should be in the view
        /// </summary>
        /// <param name="desc">The description to setup the events on</param>
        private void SetupEventsOnDescription(Description desc)
        {
            //this method gets called when a description is deleted
            desc.DescriptionDeleteEvent += (sender1, e1) =>
                {
                    //remove description from appropriate lists
                    if (desc.IsExtendedDescription)
                        ExtendedDescriptions.Remove(desc);
                    else if (!desc.IsExtendedDescription)
                        RegularDescriptions.Remove(desc);

                    AllDescriptions.Remove(desc);
                };
        }

        /// <summary>
        /// This function closes everything necessary to start fresh
        /// </summary>
        public void CloseDescriptionCollectionViewModel()
        {
            AllDescriptions.Clear();
            ExtendedDescriptions.Clear();
            RegularDescriptions.Clear();
            Recorder = GetDescriptionRecorder();
        }

        private DescriptionRecorder GetDescriptionRecorder()
        {
            var dr = new DescriptionRecorder();
            dr.DescriptionRecorded += (sender, args) => AddDescription(args.Value);
            return dr;
        }
        #endregion

        #region Event Invokation Methods

        private void OnAddDescription(Description desc)
        {
            EventHandler<DescriptionEventArgs> addDescriptionHandler = AddDescriptionEvent;
            if (addDescriptionHandler != null)
                addDescriptionHandler(this, new DescriptionEventArgs(desc));
        }
        #endregion
    }
}
