using LiveDescribe.Extensions;
using LiveDescribe.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveDescribe.Properties;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for SpaceAndDescriptionsTabControl.xaml
    /// </summary>
    public partial class SpaceAndDescriptionsTabControl : System.Windows.Controls.UserControl
    {
        public SpaceAndDescriptionsTabControl()
        {
            InitializeComponent();
            if (Defines.Zagga)
                ExtendedDescriptionsTabItem.Visibility = Visibility.Collapsed;
        }

        public void Item_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content;
            if (item is Space)
                ((Space)item).GoToThisSpaceCommand.Execute();
            else if (item is Description)
                ((Description)item).NavigateToCommand.Execute();
        }
    }
}
