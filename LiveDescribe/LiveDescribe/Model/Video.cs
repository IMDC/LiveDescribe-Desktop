using System.Runtime.CompilerServices;

namespace LiveDescribe.Model
{
    using System.ComponentModel;

    /// <summary>
    /// Video Class Represents a video
    /// </summary>
    class Video : INotifyPropertyChanged
    {
        public enum States { Playing, Paused, Recording, NotLoaded, Loaded };

        private string _path;
        private double _duration;
        private States _currentState;
        private double _currentTime;


        public Video()
        {
            _currentState = States.NotLoaded;
            _path = "";
        }

        public Video(string path)
        {
            _path = path;
            _currentState = States.NotLoaded;
        }

        #region Properties

        public States CurrentState
        {
            get
            {
                return _currentState;
            }
            set
            {
                _currentState = value;
                NotifyPropertyChanged();

            }
        }

        public string Path
        {
            set
            {
                _path = value;
                NotifyPropertyChanged();
            }
            get
            {
                return _path;
            }
        }

        public double CurrentTime
        {
            set
            {
                _currentTime = value;
                NotifyPropertyChanged();
            }
            get { return _currentTime; }
        }

        public double Duration
        {
            set
            {
                _duration = value;
                NotifyPropertyChanged();
            }
            get { return _duration; }
        }
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
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
        #endregion
    }
}
