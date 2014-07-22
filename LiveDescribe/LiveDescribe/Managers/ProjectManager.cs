using LiveDescribe.Events;
using LiveDescribe.Extensions;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace LiveDescribe.Managers
{
    public sealed class ProjectManager
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Fields
        private readonly ProjectLoader _projectLoader;
        private readonly ObservableCollection<Description> _allDescriptions;
        private readonly ObservableCollection<Description> _extendedDescriptions;
        private readonly ObservableCollection<Description> _regularDescriptions;
        private readonly ObservableCollection<Space> _spaces;
        #endregion

        #region Events
        public event EventHandler<EventArgs<Project>> ProjectLoaded;
        public event EventHandler ProjectSaved;
        public event EventHandler ProjectClosed;
        #endregion

        #region Constructor
        public ProjectManager(LoadingViewModel loadingViewModel)
        {
            _allDescriptions = new ObservableCollection<Description>();
            _allDescriptions.CollectionChanged += DescriptionsOnCollectionChanged;

            _extendedDescriptions = new ObservableCollection<Description>();
            _extendedDescriptions.CollectionChanged +=
                ObservableCollectionIndexer<Description>.CollectionChangedListener;

            _regularDescriptions = new ObservableCollection<Description>();
            _regularDescriptions.CollectionChanged +=
                ObservableCollectionIndexer<Description>.CollectionChangedListener;

            _spaces = new ObservableCollection<Space>();
            _spaces.CollectionChanged += ObservableCollectionIndexer<Space>.CollectionChangedListener;
            _spaces.CollectionChanged += SpacesOnCollectionChanged;


            _projectLoader = new ProjectLoader(loadingViewModel);
            _projectLoader.DescriptionsLoaded += (sender, args) => AllDescriptions.AddRange(args.Value);
            _projectLoader.SpacesLoaded += (sender, args) => Spaces.AddRange(args.Value);
            _projectLoader.SpacesAudioAnalysisCompleted += (sender, args) => Spaces.AddRange(args.Value);
            _projectLoader.ProjectLoaded += (sender, args) =>
            {
                Project = args.Value;
                OnProjectLoaded(Project);
            };
        }

        #endregion

        #region Properties
        public Project Project { private set; get; }

        public ObservableCollection<Description> AllDescriptions
        {
            get { return _allDescriptions; }
        }

        public ObservableCollection<Description> ExtendedDescriptions
        {
            get { return _extendedDescriptions; }
        }

        public ObservableCollection<Description> RegularDescriptions
        {
            get { return _regularDescriptions; }
        }

        public ObservableCollection<Space> Spaces
        {
            get { return _spaces; }
        }

        #endregion

        #region Load Project Methods

        public void LoadProject(Project project)
        {
            _projectLoader.StartLoadingProject(project);
        }
        #endregion

        #region Save Project
        public void SaveProject()
        {
            FileWriter.WriteProjectFile(Project);

            if (!Directory.Exists(Project.Folders.Cache))
                Directory.CreateDirectory(Project.Folders.Cache);

            FileWriter.WriteWaveFormHeader(Project, Project.Waveform.Header);
            FileWriter.WriteWaveFormFile(Project, Project.Waveform.Data);
            FileWriter.WriteDescriptionsFile(Project, AllDescriptions);
            FileWriter.WriteSpacesFile(Project, Spaces);

            OnProjectSaved();
        }
        #endregion

        public void CloseProject()
        {
            AllDescriptions.Clear();
            ExtendedDescriptions.Clear();
            RegularDescriptions.Clear();

            Spaces.Clear();

            OnProjectClosed();
        }

        #region AddDescriptionEventHandlers
        private void DescriptionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action != NotifyCollectionChangedAction.Add)
                return;

            foreach (Description d in args.NewItems)
            {
#if ZAGGA
                if (d.IsExtendedDescription)
                    continue;
#endif

                if (!d.IsExtendedDescription)
                    RegularDescriptions.Add(d);
                else
                    ExtendedDescriptions.Add(d);

                AddDescriptionEventHandlers(d);
            }
        }

        /// <summary>
        /// Method to setup events on a descriptions no graphics setup should be included in here,
        /// that should be in the view
        /// </summary>
        /// <param name="desc">The description to setup the events on</param>
        private void AddDescriptionEventHandlers(Description desc)
        {
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
        #endregion

        #region AddSpaceEventHandlers
        private void AddSpaceEventHandlers(Space s)
        {
            s.SpaceDeleteEvent += (sender, args) => Spaces.Remove(s);
        }

        private void SpacesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action != NotifyCollectionChangedAction.Add)
                return;

            foreach (Space space in args.NewItems)
                AddSpaceEventHandlers(space);
        }

        #endregion

        #region Event Invokations
        private void OnProjectLoaded(Project project)
        {
            var handler = ProjectLoaded;
            if (handler != null) handler(this, new EventArgs<Project>(project));
        }

        private void OnProjectSaved()
        {
            var handler = ProjectSaved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnProjectClosed()
        {
            var handler = ProjectClosed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
