﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Utilities;
using LiveDescribe.Events;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework;
using Microsoft.TeamFoundation.MVVM;

namespace LiveDescribe.Model
{
    public class Description: INotifyPropertyChanged
    {
        private AudioUtility _audioutility;
        private string _filename;
        private string _descriptiontext;
        private bool _isextendeddescription;
        private double _startwavefiletime;
        private double _endwavefiletime;
        private double _actuallength;
        private double _startinvideo;
        private double _endinvideo;
        private double _X;
        private double _Y;
        private double _width;
        private double _height;
        private bool _isselected;
        private NAudio.Wave.WaveOutEvent _soundOutput;

        public EventHandler DescriptionMouseDownEvent;
        public EventHandler DescriptionMouseUpEvent;
        public EventHandler DescriptionMouseMoveEvent;

        public Description(string filename, double startwavefiletime, double endwavefiletime, double startinvideo, bool extendedDescription)
        {
            FileName = filename;
            DescriptionText = filename;
            _startwavefiletime = startwavefiletime;
            _endwavefiletime = endwavefiletime;
            _startinvideo = startinvideo;
            _endinvideo = startinvideo + (endwavefiletime - startwavefiletime);
            IsExtendedDescription = extendedDescription;
            DescriptionMouseDownCommand = new RelayCommand(DescriptionMouseDown, param => true);
            DescriptionMouseUpCommand = new RelayCommand(DescriptionMouseUp, param => true);
            DescriptionMouseMoveCommand = new RelayCommand(DescriptionMouseMove, param => true);
        }

        #region Properties
        /// <summary>
        /// Keeps track of the description's X values
        /// </summary>
        public double X
        {
            set
            {
                _X = value;
                NotifyPropertyChanged("X");
            }
            get
            {
                return _X;
            }
        }

        /// <summary>
        /// Keeps track of the description's Y value
        /// </summary>
        public double Y
        {
            set
            {
                _Y = value;
                NotifyPropertyChanged("Y");
            }
            get
            {
                return _Y;
            }
        }
        /// <summary>
        /// Keeps track of the height of the description
        /// </summary>
        public double Height
        {
            set
            {
                _height = value;
                NotifyPropertyChanged("Height");
            }
            get
            {
                return _height;
            }
        }

        /// <summary>
        /// Keeps track of the Width of the description
        /// </summary>
        public double Width
        {
            set
            {
                _width = value;
                NotifyPropertyChanged("Width");
            }
            get
            {
                return _width;
            }
        }

        /// <summary>
        /// Audio Utility that contains information about the wav description file
        /// </summary>
        public AudioUtility AudioUtility
        {
            set
            {
                _audioutility = value;
                NotifyPropertyChanged("AudioUtility");
            }
            get
            {
                return _audioutility;
            }
        }
        /// <summary>
        /// Filename of the wav file
        /// </summary>
        public string FileName
        {
            set
            {
                _filename = value;
                NotifyPropertyChanged("FileName");
            }
            get
            {
                return _filename;
            }
        }

        /// <summary>
        /// Whether the description is extended or not
        /// </summary>
        public bool IsExtendedDescription
        {
            set
            {
                _isextendeddescription = value;
                NotifyPropertyChanged("IsExtendedDescription");
            }
            get
            {
                return _isextendeddescription;
            }
        }

        /// <summary>
        /// The time that the description starts within the wav file
        /// for example it might start at 5 seconds instead of 0 (at the beginning)
        /// </summary>
        public double StartWaveFileTime
        {
            set
            {
                _startwavefiletime = value;
                NotifyPropertyChanged("StartWaveFileTime");
            }
            get
            {
                return _startwavefiletime;
            }
        }
        /// <summary>
        /// The time that the description ends within the wav file
        /// for example the description might end before the wav file actually finishes
        /// </summary>
        public double EndWaveFileTime
        {
            set
            {
                _endwavefiletime = value;
                NotifyPropertyChanged("EndWaveFileTime");
            }
            get
            {
                return _endwavefiletime;
            }
        }
        /// <summary>
        /// Actualy Length of the description
        /// </summary>
        public double ActualLength
        {
            private set
            {
                _actuallength = value;
                NotifyPropertyChanged("ActualLength");
            }
            get
            {
                return _actuallength;
            }
        }
        /// <summary>
        /// The time in the video that the description starts
        /// </summary>
        public double StartInVideo
        {
            set
            {
                _startinvideo = value;
                NotifyPropertyChanged("StartInVideo");
            }
            get
            {
                return _startinvideo;
            }
        }
        /// <summary>
        /// The time in the video that the description ends
        /// </summary>
        public double EndInVideo
        {
            set
            {
                _endinvideo = value;
                NotifyPropertyChanged("EndInVideo");
            }
            get
            {
                return _endinvideo;
            }
        }

        public bool IsSelected
        {
            set
            {
                _isselected = value;
                NotifyPropertyChanged("IsSelected");
            }
            get
            {
                return _isselected;
            }
        }

        public NAudio.Wave.WaveOutEvent SoundOutput
        {
            set
            {
                _soundOutput = value;
                NotifyPropertyChanged("SoundOutput");
            }
            get
            {
                return _soundOutput;
            }
        }

        public string DescriptionText
        {
            set
            {
                _descriptiontext = value;
                NotifyPropertyChanged("DescriptionText");
            }
            get
            {
                return _descriptiontext;
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for the DescriptionMouseDown Command
        /// </summary>
        public RelayCommand DescriptionMouseDownCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and getter for the DescriptionMouseUp Command
        /// </summary>
        public RelayCommand DescriptionMouseUpCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and getter for the DescriptionMouseMove Command
        /// </summary>
        public RelayCommand DescriptionMouseMoveCommand
        {
            get;
            private set;
        }
        #endregion

        #region Binding Functions
        /// <summary>
        /// Called when the mouse is down on this description
        /// </summary>
        /// <param name="param"></param>
        public void DescriptionMouseDown(object param)
        {
            EventHandler handler = DescriptionMouseDownEvent;
            IsSelected = true;
            Console.WriteLine("Mouse Down");
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is up on this description
        /// </summary>
        /// <param name="param"></param>
        public void DescriptionMouseUp(object param)
        {
            EventHandler handler = DescriptionMouseUpEvent;
            IsSelected = false;
            Console.WriteLine("MOUSE UP");
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the mouse is moving on this description
        /// </summary>
        /// <param name="param"></param>
        public void DescriptionMouseMove(object param)
        {
            EventHandler handler = DescriptionMouseMoveEvent;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
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
