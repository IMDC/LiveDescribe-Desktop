using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LiveDescribe.Model
{
    public class ColourScheme : INotifyPropertyChanged
    {
        public static readonly ColourScheme DefaultColourScheme = new ColourScheme
        {
            _regularDescriptionColour = Color.FromArgb(0x40, 0x00, 0x80, 0x00),
            _extendedDescriptionColour = Color.FromArgb(0x40, 0xff, 0x0, 0x0),
            _spaceColour = Color.FromArgb(040, 0x69, 0x69, 0x69),
            _completedSpaceColour = Color.FromArgb(0x28, 0xff, 0xff, 0x00),
            _selectedItemColour = Color.FromArgb(0x40, 0xff, 0xff, 0x00),
        };

        #region Fields
        private Color _regularDescriptionColour;
        private Color _extendedDescriptionColour;
        private Color _spaceColour;
        private Color _completedSpaceColour;
        private Color _selectedItemColour;
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        public ColourScheme() { }

        public ColourScheme(ColourScheme otherScheme)
        {
            _regularDescriptionColour = otherScheme.RegularDescriptionColour;
            _extendedDescriptionColour = otherScheme.ExtendedDescriptionColour;
            _spaceColour = otherScheme.SpaceColour;
            _completedSpaceColour = otherScheme.CompletedSpaceColour;
            _selectedItemColour = otherScheme.SelectedItemColour;
        }
        #endregion

        #region Properties
        public Color RegularDescriptionColour
        {
            get { return _regularDescriptionColour; }
            set
            {
                _regularDescriptionColour = value;
                NotifyPropertyChanged();
            }
        }

        public Color ExtendedDescriptionColour
        {
            get { return _extendedDescriptionColour; }
            set
            {
                _extendedDescriptionColour = value;
                NotifyPropertyChanged();
            }
        }

        public Color SpaceColour
        {
            get { return _spaceColour; }
            set
            {
                _spaceColour = value;
                NotifyPropertyChanged();
            }
        }

        public Color CompletedSpaceColour
        {
            get { return _completedSpaceColour; }
            set
            {
                _completedSpaceColour = value;
                NotifyPropertyChanged();
            }
        }

        public Color SelectedItemColour
        {
            get { return _selectedItemColour; }
            set
            {
                _selectedItemColour = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Event Invokation
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
