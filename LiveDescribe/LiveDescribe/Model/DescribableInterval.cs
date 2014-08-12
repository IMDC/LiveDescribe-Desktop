using LiveDescribe.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LiveDescribe.Model
{
    public abstract class DescribableInterval : IDescribableInterval, IListIndexable, INotifyPropertyChanged
    {
        //All units of time is in milliseconds
        #region Instance variables
        private string _text;
        private double _startinvideo;
        private double _endinvideo;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private bool _isSelected;
        private int _index;
        private Color _colour;
        private double _duration;

        #endregion

        #region Properties
        /// <summary>
        /// Keeps track of the description's X values
        /// </summary>
        [JsonIgnore]
        public double X
        {
            set
            {
                _x = value;
                NotifyPropertyChanged();
            }
            get { return _x; }
        }

        /// <summary>
        /// Keeps track of the description's Y value
        /// </summary>
        [JsonIgnore]
        public double Y
        {
            set
            {
                _y = value;
                NotifyPropertyChanged();
            }
            get { return _y; }
        }
        /// <summary>
        /// Keeps track of the height of the description
        /// </summary>
        [JsonIgnore]
        public double Height
        {
            set
            {
                _height = value;
                NotifyPropertyChanged();
            }
            get { return _height; }
        }

        /// <summary>
        /// Keeps track of the Width of the description
        /// </summary>
        [JsonIgnore]
        public double Width
        {
            set
            {
                _width = value;
                NotifyPropertyChanged();
            }
            get { return _width; }
        }

        /// <summary>
        /// The time in the video that the description starts
        /// </summary>
        public double StartInVideo
        {
            set
            {
                _startinvideo = value;
                NotifyPropertyChanged();
            }
            get { return _startinvideo; }
        }
        /// <summary>
        /// The time in the video that the description ends
        /// </summary>
        public double EndInVideo
        {
            set
            {
                _endinvideo = value;
                NotifyPropertyChanged();
            }
            get { return _endinvideo; }
        }

        [JsonIgnore]
        public bool IsSelected
        {
            set
            {
                _isSelected = value;
                NotifyPropertyChanged();
            }
            get { return _isSelected; }
        }

        public string Text
        {
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
            get { return _text; }
        }

        public void SetStartAndEndInVideo(double startInVideo, double endInVideo)
        {
            _startinvideo = startInVideo;
            _endinvideo = endInVideo;
            NotifyPropertyChanged();
        }

        /// <summary>
        /// The length of the span the description is set to play in the video.
        /// </summary>
        [JsonIgnore]
        public virtual double Duration
        {
            set
            {
                _duration = value;
                NotifyPropertyChanged();
            }
            get { return _duration; }
        }

        [JsonIgnore]
        public int Index
        {
            set
            {
                _index = value;
                NotifyPropertyChanged();
            }
            get { return _index; }
        }

        [JsonIgnore]
        public Color Colour
        {
            set
            {
                _colour = value;
                NotifyPropertyChanged();
            }
            get { return _colour; }
        }
        #endregion

        #region Methods
        public abstract void SetColour();
        #endregion

        #region PropertyChanged
        /// <summary>
        /// An event that notifies a subscriber that a property in this class has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property changed.</param>
        protected virtual void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
        #endregion
    }
}
