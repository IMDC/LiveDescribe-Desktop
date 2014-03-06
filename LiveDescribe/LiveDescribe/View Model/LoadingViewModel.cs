using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace LiveDescribe.View_Model
{
    public class LoadingViewModel : ViewModelBase
    {
        #region Instance Variables
        private string _text;
        private double _value;
        private double _max;
        private bool _visible;
        #endregion

        public LoadingViewModel(double max, string text, double initialValue, bool visible)
        {
            Max = max;
            Text = text;
            Value = initialValue;
            Visible = visible;
        }

        #region Binding Properties
        /// <summary>
        /// This property represents the text of the loading screen
        /// </summary>
        public string Text
        {
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// This property represents the current value of the loading screen
        /// </summary>
        public double Value
        {
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
            get
            {
                return _value;
            }
        }
        /// <summary>
        /// This property represents the max value of the loading screen
        /// </summary>
        public double Max
        {
            set
            {
                _max = value;
                RaisePropertyChanged("Max");
            }
            get
            {
                return _max;
            }
        }

        /// <summary>
        /// This property represents whether it is visible or not
        /// </summary>
        public bool Visible
        {
            set
            {
                _visible = value;
                RaisePropertyChanged("Visible");
            }
            get
            {
                return _visible;
            }
        }
        #endregion
    }
}