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
        #endregion

        #region Constructors
        public SpaceCollectionViewModel(ILiveDescribePlayer videoPlayer, ProjectManager projectManager)
        {
            _projectManager = projectManager;
            _indexer = new ObservableCollectionIndexer<Space>(Spaces);

            _projectManager.SpacesAudioAnalysisCompleted += (sender, args) => Spaces.AddRange(args.Value);
            _projectManager.SpacesLoaded += (sender, args) => Spaces.AddRange(args.Value);
        }
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
    }
}
