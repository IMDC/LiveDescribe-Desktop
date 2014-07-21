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
        private ObservableCollection<Description> _regularDescriptions;  //this list only contains all the regular descriptions this list should only be used to bind to the list of regular descriptions
        #endregion

        #region Event Handlers
        public event EventHandler<DescriptionEventArgs> AddDescriptionEvent;
        #endregion

        #region Constructors
        public DescriptionCollectionViewModel(ProjectManager projectManager)
        {
            AllDescriptions = new ObservableCollection<Description>();
            RegularDescriptions = new ObservableCollection<Description>();
            RegularDescriptions.CollectionChanged += ObservableCollectionIndexer<Description>.CollectionChangedListener;

            ExtendedDescriptions = new ObservableCollection<Description>();
            ExtendedDescriptions.CollectionChanged += ObservableCollectionIndexer<Description>.CollectionChangedListener;

            projectManager.Descriptions = AllDescriptions;
            projectManager.DescriptionsLoaded += (sender, args) => AddDescriptions(args.Value);
        }
        #endregion

        #region Properties
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
