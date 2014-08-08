using LiveDescribe.Events;
using LiveDescribe.Extensions;
using LiveDescribe.Model;
using LiveDescribe.Properties;
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
        private bool _isProjectModified;
        private readonly ObservableCollection<Description> _allDescriptions;
        private readonly ObservableCollection<Description> _extendedDescriptions;
        private readonly ObservableCollection<Description> _regularDescriptions;
        private readonly ObservableCollection<Space> _spaces;
        private readonly UndoRedoManager _undoRedoManager;
        #endregion

        #region Events
        public event EventHandler<EventArgs<Project>> ProjectLoaded;
        public event EventHandler ProjectSaved;
        public event EventHandler ProjectClosed;
        public event EventHandler ProjectModifiedStateChanged;
        #endregion

        #region Constructor
        public ProjectManager(LoadingViewModel loadingViewModel, UndoRedoManager undoRedoManager)
        {
            _undoRedoManager = undoRedoManager;

            _allDescriptions = new ObservableCollection<Description>();
            _allDescriptions.CollectionChanged += DescriptionsOnCollectionChanged;

            _extendedDescriptions = new ObservableCollection<Description>();
            _extendedDescriptions.CollectionChanged +=
                ObservableCollectionIndexer<Description>.CollectionChangedListener;
            _extendedDescriptions.CollectionChanged += ObservableCollection_ProjectModifiedHandler;

            _regularDescriptions = new ObservableCollection<Description>();
            _regularDescriptions.CollectionChanged +=
                ObservableCollectionIndexer<Description>.CollectionChangedListener;
            _regularDescriptions.CollectionChanged += ObservableCollection_ProjectModifiedHandler;

            _spaces = new ObservableCollection<Space>();
            _spaces.CollectionChanged += ObservableCollectionIndexer<Space>.CollectionChangedListener;
            _spaces.CollectionChanged += SpacesOnCollectionChanged;
            _spaces.CollectionChanged += ObservableCollection_ProjectModifiedHandler;

            _projectLoader = new ProjectLoader(loadingViewModel);
            _projectLoader.DescriptionsLoaded += (sender, args) => AddDescriptions(args.Value);
            _projectLoader.SpacesLoaded += (sender, args) => Spaces.AddRange(args.Value);
            _projectLoader.SpacesAudioAnalysisCompleted += (sender, args) => Spaces.AddRange(args.Value);
            _projectLoader.ProjectLoaded += (sender, args) =>
            {
                Project = args.Value;
                IsProjectModified = false;
                OnProjectLoaded(Project);
            };
        }

        #endregion

        #region Properties
        public Project Project { private set; get; }

        public bool HasProjectLoaded
        {
            get { return Project != null; }
        }

        /// <summary>
        /// Keeps track of whether the project has been modified or not by the program. This will be
        /// true iff there is a project loaded already.
        /// </summary>
        public bool IsProjectModified
        {
            private set
            {
                if (_isProjectModified != value)
                {
                    _isProjectModified = HasProjectLoaded && value;
                    OnProjectModifiedStateChanged();
                }
            }
            get { return HasProjectLoaded && _isProjectModified; }
        }

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

            IsProjectModified = false;
            OnProjectSaved();
        }
        #endregion

        #region Close Project
        public void CloseProject()
        {
            AllDescriptions.Clear();
            ExtendedDescriptions.Clear();
            RegularDescriptions.Clear();

            Spaces.Clear();

            Log.InfoFormat("Closed Project \"{0}\"", Project.ProjectName);

            Project = null;
            IsProjectModified = false;

            OnProjectClosed();
        }
        #endregion

        #region Add and Remove DescriptionEventHandlers
        private void DescriptionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Description d in args.NewItems)
                    AddDescriptionEventHandlers(d);
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Description d in args.OldItems)
                    RemoveDescriptionEventHandlers(d);
            }
        }

        /// <summary>
        /// Method to setup events on a descriptions no graphics setup should be included in here,
        /// that should be in the view
        /// </summary>
        /// <param name="desc">The description to setup the events on</param>
        private void AddDescriptionEventHandlers(Description desc)
        {
            desc.DescriptionDeleteEvent += DescriptionDelete;
        }

        private void RemoveDescriptionEventHandlers(Description desc)
        {
            desc.DescriptionDeleteEvent -= DescriptionDelete;
        }

        private void DescriptionDelete(object sender, EventArgs e)
        {
            var desc = (Description)sender;
            RemoveDescriptionAndTrackForUndo(desc);
        }
        #endregion

        #region public methods
        public void AddSpaceAndTrackForUndo(Space space)
        {
            Spaces.Add(space);
            _undoRedoManager.InsertSpaceForInsertUndoRedo(Spaces, space);
        }

        public void AddDescriptionAndTrackForUndo(Description desc)
        {
            if (Defines.Zagga && desc.IsExtendedDescription)
                return;

            AllDescriptions.Add(desc);
            if (!desc.IsExtendedDescription)
            {
                RegularDescriptions.Add(desc);
                _undoRedoManager.InsertDescriptionForInsertUndoRedo(AllDescriptions, RegularDescriptions, desc);
            }
            else if (desc.IsExtendedDescription)
            {
                ExtendedDescriptions.Add(desc);
                _undoRedoManager.InsertDescriptionForInsertUndoRedo(AllDescriptions, ExtendedDescriptions, desc);
            }
        }

        public void RemoveDescriptionAndTrackForUndo(Description desc)
        {
            if (desc.IsExtendedDescription)
            {
                ExtendedDescriptions.Remove(desc);
                _undoRedoManager.InsertDescriptionForDeleteUndoRedo(AllDescriptions, ExtendedDescriptions, desc);
            }
            else if (!desc.IsExtendedDescription)
            {
                RegularDescriptions.Remove(desc);
                _undoRedoManager.InsertDescriptionForDeleteUndoRedo(AllDescriptions, RegularDescriptions, desc);
            }

            AllDescriptions.Remove(desc);
        }

        public void AddDescription(Description desc)
        {
            if (Defines.Zagga && desc.IsExtendedDescription)
                return;

            AllDescriptions.Add(desc);

            if (!desc.IsExtendedDescription)
                RegularDescriptions.Add(desc);
            else
                ExtendedDescriptions.Add(desc);
        }

        public void AddDescriptions(List<Description> descriptions)
        {
            foreach (Description d in descriptions)
                AddDescription(d);
        }
        #endregion

        #region Add And Remove SpaceEventHandlers
        private void AddSpaceEventHandlers(Space s)
        {
            s.SpaceDeleteEvent += SpaceDeleted;
        }

        private void SpaceDeleted(object sender, EventArgs e)
        {
            var s = (Space)sender;
            Spaces.Remove(s);
            _undoRedoManager.InsertSpaceForDeleteUndoRedo(Spaces, s);
        }

        private void SpacesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Space space in args.NewItems)
                    AddSpaceEventHandlers(space);
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Space space in args.OldItems)
                    RemoveSpaceEventHandlers(space);
            }
        }

        private void RemoveSpaceEventHandlers(Space space)
        {
            space.SpaceDeleteEvent -= SpaceDeleted;
        }

        #endregion

        #region Project Modification Event Handlers
        /// <summary>
        /// Adds a propertychanged handler to each new element of an observable collection, and
        /// removes one from each removed element.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Args</param>
        private void ObservableCollection_ProjectModifiedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var notifier = item as INotifyPropertyChanged;

                    if (notifier != null)
                        notifier.PropertyChanged += ObservableCollectionElement_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var notifier = item as INotifyPropertyChanged;

                    if (notifier != null)
                        notifier.PropertyChanged -= ObservableCollectionElement_PropertyChanged;
                }
            }

            IsProjectModified = true;
        }

        /// <summary>
        /// Flags the current project as modified, so that the program (and user) know that it has
        /// been modified since the last save.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Args</param>
        private void ObservableCollectionElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //Fallthrough cases
                case "AudioFile":
                case "IsExtendedDescription":
                case "StartWaveFileTime":
                case "EndWaveFileTime":
                case "ActualLength":
                case "StartInVideo":
                case "EndInVideo":
                case "Text":
                case "AudioData":
                case "Header":
                case "IsRecordedOver":
                    IsProjectModified = true;
                    break;
            }
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

        private void OnProjectModifiedStateChanged()
        {
            var handler = ProjectModifiedStateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
