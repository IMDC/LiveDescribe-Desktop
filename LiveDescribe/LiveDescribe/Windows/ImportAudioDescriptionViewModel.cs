using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Windows.Input;

namespace LiveDescribe.Windows
{
    public class ImportAudioDescriptionViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Fields
        private string _descriptionPath;
        private readonly double _videoDurationMilliseconds;
        private double _startInVideo;
        private double _endInVideo;
        private string _text;
        private bool _isStartInVideoTextBoxEnabled;
        private readonly ProjectManager _projectManager;

        public bool? DialogResult { set; get; }
        private double DescriptionLengthInMilliseconds { set; get; }
        #endregion

        #region Events

        public EventHandler OnImportDescription;
        #endregion

        public ImportAudioDescriptionViewModel(ProjectManager projectManager, double videoDurationMilliseconds)
        {
            ChooseDescriptionWavFileCommand = new RelayCommand(ChooseDescriptionWavFile);
            ImportAudioDescriptionCommand = new RelayCommand(ImportAudioDescription, CanImportDescription);
            IsStartInVideoTextBoxEnabled = false;
            _projectManager = projectManager;
            _videoDurationMilliseconds = videoDurationMilliseconds;
        }

        #region Properties

        public string DescriptionPath
        {
            set
            {
                _descriptionPath = value;
                RaisePropertyChanged();
            }
            get { return _descriptionPath; }
        }

        public double StartInVideo
        {
            set
            {
                if (double.IsNaN(value) || value > _videoDurationMilliseconds ||
                    value + DescriptionLengthInMilliseconds > _videoDurationMilliseconds)
                    return;
                _startInVideo = value;
                EndInVideo = _startInVideo + DescriptionLengthInMilliseconds;
                RaisePropertyChanged();
            }
            get { return _startInVideo; }
        }

        public double EndInVideo
        {
            set
            {
                if (double.IsNaN(value) || value > _videoDurationMilliseconds ||
                    value + DescriptionLengthInMilliseconds > _videoDurationMilliseconds)
                    return;
                _endInVideo = value;
                RaisePropertyChanged();
            }
            get { return _endInVideo; }
        }

        public string Text
        {
            set
            {
                _text = value;
                RaisePropertyChanged();
            }
            get { return _text; }
        }

        private bool CanImportDescription()
        {
            return !string.IsNullOrWhiteSpace(DescriptionPath);
        }

        public bool IsStartInVideoTextBoxEnabled
        {
            set
            {
                _isStartInVideoTextBoxEnabled = value;
                RaisePropertyChanged();
            }
            get { return _isStartInVideoTextBoxEnabled; }
        }

        #endregion

        #region Command and Command Functions
        public ICommand ChooseDescriptionWavFileCommand { private set; get; }

        public ICommand ImportAudioDescriptionCommand { private set; get; }

        private void ChooseDescriptionWavFile()
        {
            var fileChooser = new OpenFileDialog();
            fileChooser.Filter = "Wav Files (*.wav)|*.wav";
            bool? dialogSuccess = fileChooser.ShowDialog();

            if (dialogSuccess == true)
            {
                DescriptionPath = fileChooser.FileName;
                IsStartInVideoTextBoxEnabled = true;
                DescriptionLengthInMilliseconds = GetDescriptionLengthInMilliseconds();
                EndInVideo = DescriptionLengthInMilliseconds;
                Log.Info("Description Path Chosen: " + DescriptionPath + " with length: " + DescriptionLengthInMilliseconds);
            }
        }

        private void ImportAudioDescription()
        {
            var desc = new Description(ProjectFile.FromAbsolutePath(DescriptionPath, _projectManager.Project.Folders.Project),
                    0, DescriptionLengthInMilliseconds, StartInVideo, false) { Text = Text };

            _projectManager.AddDescriptionAndTrackForUndo(desc);
            var handler = OnImportDescription;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private double GetDescriptionLengthInMilliseconds()
        {
            var read = new WaveFileReader(DescriptionPath);
            return read.TotalTime.TotalMilliseconds;
        }
        #endregion
    }
}
