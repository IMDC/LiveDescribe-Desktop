using GalaSoft.MvvmLight;

namespace LiveDescribe.Controls.UserControls
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
                RaisePropertyChanged();
            }
            get { return _text; }
        }

        /// <summary>
        /// This property represents the current value of the loading screen
        /// </summary>
        public double Value
        {
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
            get { return _value; }
        }
        /// <summary>
        /// This property represents the max value of the loading screen
        /// </summary>
        public double Max
        {
            set
            {
                _max = value;
                RaisePropertyChanged();
            }
            get { return _max; }
        }

        /// <summary>
        /// This property represents whether it is visible or not
        /// </summary>
        public bool Visible
        {
            set
            {
                _visible = value;
                RaisePropertyChanged();
            }
            get { return _visible; }
        }
        #endregion

        /// <summary>
        /// Sets the progress of the LoadingViewModel along with a message, in the form of "message:
        /// progress%". An example with params "Loading" and "5" would be "Loading: 5%"
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="progress">LoadingProgress.</param>
        public void SetProgress(string message, double progress)
        {
            Text = string.Format("{0}: {1}%", message, progress);
            Value = progress;
        }
    }
}