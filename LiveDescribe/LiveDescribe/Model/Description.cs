using GalaSoft.MvvmLight.Command;
using LiveDescribe.Utilities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LiveDescribe.Model
{
    public class Description : DescribableInterval
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        //All units of time is in milliseconds
        #region Instance variables
        private ProjectFile _audioFile;
        private bool _isextendeddescription;
        private bool _isPlaying;
        private double _startwavefiletime;
        private double _endwavefiletime;
        private Waveform _waveform;
        private RenderTargetBitmap _waveformImage;
        #endregion

        #region Constructors
        public Description(ProjectFile filepath, double startwavefiletime, double endwavefiletime,
            double startinvideo, bool extendedDescription)
            : this()
        {
            AudioFile = filepath;

            Text = Path.GetFileNameWithoutExtension(filepath);
            IsExtendedDescription = extendedDescription;

            //I specifically use the instance variables rather than the properties
            //because the property events can possibly be caught in the view
            //leading to an uneeded amount of changes to the description graphics
            _startwavefiletime = startwavefiletime;
            _endwavefiletime = endwavefiletime;
            StartInVideo = startinvideo;

            if (!extendedDescription)
                EndInVideo = startinvideo + (endwavefiletime - startwavefiletime);
            else
                EndInVideo = startinvideo;
        }

        public Description()
        {
            LockedInPlace = false;

            MouseDownCommand = new RelayCommand<MouseEventArgs>(OnMouseDown, param => true);
            NavigateToCommand = new RelayCommand(OnNavigateToDescriptionRequested, () => true);
            MouseUpCommand = new RelayCommand<MouseEventArgs>(OnMouseUp, param => true);
            DeleteCommand = new RelayCommand(OnDeleteRequested, () => true);
            MouseMoveCommand = new RelayCommand<MouseEventArgs>(OnMouseMove, param => true);

            OpenWinFileExplorerToFile = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    string args = string.Format("/Select, {0}", AudioFile);
                    var pfi = new ProcessStartInfo("Explorer.exe", args);
                    Process.Start(pfi);
                });
        }
        #endregion

        #region Commands
        public ICommand OpenWinFileExplorerToFile { get; private set; }
        #endregion

        #region Properties
        [JsonIgnore]
        public override double Duration
        {
            get { return EndInVideo - StartInVideo; }
            /* You should not be able to change the Duration of a description because it is based
             * on the length of the audio file. This might be changed later with description audio
             * trimming.
             */
            protected set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Filename of the wav file
        /// </summary>
        public ProjectFile AudioFile
        {
            set
            {
                _audioFile = value;
                NotifyPropertyChanged();
            }
            get { return _audioFile; }
        }

        /// <summary>
        /// Whether the description is extended or not
        /// </summary>
        public bool IsExtendedDescription
        {
            set
            {
                _isextendeddescription = value;
                NotifyPropertyChanged();
            }
            get { return _isextendeddescription; }
        }

        /// <summary>
        /// The time that the description starts within the wav file for example it might start at 5
        /// seconds instead of 0 (at the beginning)
        /// </summary>
        public double StartWaveFileTime
        {
            set
            {
                _startwavefiletime = value;
                NotifyPropertyChanged();
            }
            get { return _startwavefiletime; }
        }
        /// <summary>
        /// The time that the description ends within the wav file for example the description might
        /// end before the wav file actually finishes
        /// </summary>
        public double EndWaveFileTime
        {
            set
            {
                _endwavefiletime = value;
                NotifyPropertyChanged();
            }
            get { return _endwavefiletime; }
        }

        [JsonIgnore]
        public bool IsPlaying
        {
            set
            {
                _isPlaying = value;
                NotifyPropertyChanged();
            }
            get { return _isPlaying; }
        }

        /// <summary>
        /// The length of time the wave file is set to play for.
        /// </summary>
        [JsonIgnore]
        public double WaveFileDuration
        {
            get { return _endwavefiletime - _startwavefiletime; }
        }

        [JsonIgnore]
        public RenderTargetBitmap WaveformImage
        {
            get { return _waveformImage; }
            set
            {
                _waveformImage = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public Waveform Waveform
        {
            get { return _waveform; }
            set
            {
                _waveform = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Methods

        public void GenerateWaveForm()
        {
            var sampler = new AudioWaveFormSampler(AudioFile);
            Waveform = sampler.CreateWaveform();
        }
        #endregion

        #region Property Changed
        protected override void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);
        }
        #endregion
    }
}
