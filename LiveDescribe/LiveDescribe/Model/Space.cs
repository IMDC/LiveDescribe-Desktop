using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.Model
{
    public class Space
    {
        #region Instance Variables
        private double _startTime;
        private string _text;
        private double _endTime;
        private double _length;
        #endregion

        #region Properties
        public string Text
        {
            set
            {
                _text = value;
                NotifyPropertyChanged("Text");
            }
            get
            {
                return _text;
            }
        }

        public double StartTime
        {
            set
            {
                _startTime = value;
                NotifyPropertyChanged("StartTime");
            }
            get { return _startTime; }
        }

        public double EndTime
        {
            set
            {
                _endTime = value;
                NotifyPropertyChanged("EndTime");
            }
            get { return _endTime; }
        }

        public double Length
        {
            set
            {
                _length = value;
                NotifyPropertyChanged("Length");
            }
            get { return _length; }
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
