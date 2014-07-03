using System.Collections.Specialized;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Events;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class SpaceCollectionViewModel : ViewModelBase
    {
        public const double MinSpaceLengthInMSecs = 333;

        #region Instance Variables
        private ObservableCollection<Space> _spaces;
        private readonly ILiveDescribePlayer _videoPlayer;
        #endregion

        #region Event Handlers
        public EventHandler<SpaceEventArgs> SpaceAddedEvent;

        /// <summary>
        /// Requests to a handler what to set the StartInVideo and EndInVideo time values for the
        /// given space.
        /// </summary>
        public event EventHandler<SpaceEventArgs> RequestSpaceTime;
        #endregion

        #region Constructors
        public SpaceCollectionViewModel(ILiveDescribePlayer videoPlayer)
        {
            Spaces = new ObservableCollection<Space>();
            Spaces.CollectionChanged += (sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        EnumerateSpaces(args.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Move:
                        EnumerateSpaces(args.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        EnumerateSpaces();
                        break;
                }
            };

            _videoPlayer = videoPlayer;

            AddSpaceCommand = new RelayCommand(AddSpace, () => true);
            GetNewSpaceTime = new RelayCommand(
                canExecute: () => _videoPlayer.CurrentState != LiveDescribeVideoStates.VideoNotLoaded,
                execute: () =>
                {
                    var s = new Space();
                    OnRequestSpaceTime(s);
                    AddSpace(s);
                });
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command used for when a space is added
        /// </summary>
        public RelayCommand AddSpaceCommand { get; private set; }

        public ICommand GetNewSpaceTime { get; private set; }
        #endregion

        #region Binding Properties
        /// <summary>
        /// List that represents all the Spaces
        /// </summary>
        public ObservableCollection<Space> Spaces
        {
            set
            {
                _spaces = value;
                RaisePropertyChanged();
            }
            get { return _spaces; }
        }

        #endregion

        #region Binding Functions
        /// <summary>
        /// Method that gets called when adding a space
        /// </summary>
        public void AddSpace()
        {
            AddSpace(new Space());
        }

        public void AddSpace(Space space)
        {
            EventHandler<SpaceEventArgs> handler = SpaceAddedEvent;
            if (handler != null) handler(this, new SpaceEventArgs(space));
            Spaces.Add(space);
            SetupEventsOnSpace(space);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Setup all the events on space that don't require any info from the UI
        /// </summary>
        /// <param name="space">The space that the events are setup on</param>
        private void SetupEventsOnSpace(Space space)
        {
            space.SpaceDeleteEvent += (sender, e) => Spaces.Remove(space);
        }

        public void CloseSpaceCollectionViewModel()
        {
            Spaces.Clear();
        }

        /// <summary>
        /// Sets the indices of all the spaces in this collection from the starting index. Indices
        /// are 1-indexed.
        /// </summary>
        /// <param name="startingIndex"></param>
        private void EnumerateSpaces(int startingIndex = 0)
        {
            for (int i = startingIndex; i < Spaces.Count; i++)
            {
                Spaces[i].Index = i + 1;
            }
        }
        #endregion

        #region Event Invocation

        private void OnRequestSpaceTime(Space s)
        {
            var handler = RequestSpaceTime;
            if (handler != null) handler(this, new SpaceEventArgs(s));
        }
        #endregion
    }
}
