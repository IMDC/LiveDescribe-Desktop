using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using LiveDescribe.Utilities;

namespace LiveDescribe.ViewModel
{
    public class SpaceRecordingViewModel : ViewModelBase
    {
        #region Fields
        private Description _description;
        private Space _space;
        private DescriptionRecorder _recorder;
        private DescriptionPlayer _player;
        #endregion

        #region Events
        public event EventHandler CloseRequested;
        #endregion

        #region Constructor
        public SpaceRecordingViewModel(Space space, Project project)
        {
            InitCommands();

            _description = null;
            Space = space;
            Project = project;

            _recorder = new DescriptionRecorder();
            _recorder.DescriptionRecorded += (sender, args) => Description = args.Value;

            _player = new DescriptionPlayer();
            _player.DescriptionFinishedPlaying += (sender, args) => CommandManager.InvalidateRequerySuggested();
        }

        public void InitCommands()
        {
            RecordDescription = new RelayCommand(
                canExecute: () =>
                    Space != null
                    && _recorder.CanRecord()
                    && !_player.IsPlaying,
                execute: () =>
                {
                    if(_recorder.IsRecording)
                        _recorder.StopRecording();
                    else
                    {
                        var pf = ProjectFile.FromAbsolutePath(Project.GenerateDescriptionFilePath(),
                                Project.Folders.Descriptions);
                        _recorder.RecordDescription(pf, false, Space.StartInVideo);
                    }
                });

            PlayRecordedDescription = new RelayCommand(
                canExecute: () =>
                    Description != null
                    && _player.CanPlay(_description),
                execute: () =>
                {
                    _player.Play(_description);
                });

            SaveDescription = new RelayCommand(
                canExecute: () => Description != null,
                execute: () =>
                {
                    //Give Space text to description before exiting
                    _description.DescriptionText = _space.SpaceText;
                    OnCloseRequested();
                });
        }
        #endregion

        #region Commands
        public ICommand RecordDescription { private set; get; }
        public ICommand PlayRecordedDescription { private set; get; }
        public ICommand SaveDescription { private set; get; }
        #endregion

        #region Properties
        public Project Project { set; get; }

        public string Text
        {
            set
            {
                _space.SpaceText = value;
                RaisePropertyChanged();
            }
            get { return _space.SpaceText; }
        }

        public string TimeStamp { set; get; }

        public Description Description
        {
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
            get { return _description; }
        }

        public Space Space
        {
            set
            {
                _space = value;
                RaisePropertyChanged();
            }
            get { return _space; }
        }
        #endregion

        #region Event Invokations

        public void OnCloseRequested()
        {
            EventHandler handler = CloseRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
