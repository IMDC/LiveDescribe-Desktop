using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net.Config;
using Microsoft.Win32;
using NAudio.Wave;

namespace LiveDescribe.ViewModel
{
    public class ImportAudioDescriptionViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Fields
        private string _descriptionPath;
        private double _startInVideo;
        private double _endInVideo;
        private string _text;
        private bool _isStartInVideoTextBoxEnabled;

        public bool? DialogResult { set; get; }
        private double DescriptionLengthInMilliseconds { set; get; }
        #endregion

        public ImportAudioDescriptionViewModel()
        {
            ChooseDescriptionWavFileCommand = new RelayCommand(ChooseDescriptionWavFile);
            IsStartInVideoTextBoxEnabled = false;
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
                if (double.IsNaN(value))
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
                if (double.IsNaN(value))
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

        private double GetDescriptionLengthInMilliseconds()
        {
            var read = new WaveFileReader(DescriptionPath);
            return read.TotalTime.TotalMilliseconds;
        }
        #endregion
    }
}
