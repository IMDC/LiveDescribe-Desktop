using System.Windows.Controls;
using System.Windows;
using LiveDescribe.Properties;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for DescriptionRecordingControl.xaml
    /// </summary>
    public partial class DescriptionRecordingControl : UserControl
    {
        public DescriptionRecordingControl()
        {
            InitializeComponent();
            if (Defines.Zagga)
            {
                ExtendedDescriptionCheckBox.Visibility = Visibility.Collapsed;
                RecordButton.Margin = new Thickness(10,30,10,10);
            }
        }
    }
}
