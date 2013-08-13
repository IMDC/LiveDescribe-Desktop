namespace LiveDescribe.Model
{


    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Video Class Represents a video
    /// </summary>
    class Video: INotifyPropertyChanged
    {
        private string _path;

        public Video(string path)
        {
            this._path = path;     
        }

        #region Properties
        public string Path
        {
            set 
            {
                _path = value;
                NotifyPropertyChanged("Path");
            }
            get
            { 
                return _path; 
            }
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
        private void NotifyPropertyChanged(string propertyName)
        {
            /* Make a local copy of the event to prevent the case where the handler
             * will be set as null in-between the null check and the handler call.
             */
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
