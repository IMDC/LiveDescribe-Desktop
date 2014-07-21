using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Events;
using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class SpaceCollectionViewModel : ViewModelBase
    {
        public const double MinSpaceLengthInMSecs = 333;

        #region Instance Variables
        private readonly ProjectManager _projectManager;
        // ReSharper disable once NotAccessedField.Local
        private ObservableCollectionIndexer<Space> _indexer;
        private readonly ILiveDescribePlayer _videoPlayer;
        #endregion

        #region Event Handlers

        /// <summary>
        /// Requests to a handler what to set the StartInVideo and EndInVideo time values for the
        /// given space.
        /// </summary>
        public event EventHandler<SpaceEventArgs> RequestSpaceTime;
        #endregion

        #region Constructors
        public SpaceCollectionViewModel(ILiveDescribePlayer videoPlayer, ProjectManager projectManager)
        {
            _projectManager = projectManager;
            _indexer = new ObservableCollectionIndexer<Space>(Spaces);

            _videoPlayer = videoPlayer;

            GetNewSpaceTime = new RelayCommand(
                canExecute: () => _videoPlayer.CurrentState != LiveDescribeVideoStates.VideoNotLoaded,
                execute: () =>
                {
                    var s = new Space();
                    OnRequestSpaceTime(s);
                    Spaces.Add(s);
                });

            _projectManager.SpacesAudioAnalysisCompleted += (sender, args) => Spaces.AddRange(args.Value);
            _projectManager.SpacesLoaded += (sender, args) => Spaces.AddRange(args.Value);
        }
        #endregion

        #region Commands
        public ICommand GetNewSpaceTime { get; private set; }
        #endregion

        #region Binding Properties
        /// <summary>
        /// List that represents all the Spaces
        /// </summary>
        public ObservableCollection<Space> Spaces
        {
            get { return _projectManager.Spaces; }
        }

        #endregion

        #region Methods

        public void CloseSpaceCollectionViewModel()
        {
            Spaces.Clear();
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
