using LiveDescribe.Extensions;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for SpaceAndDescriptionsTabControl.xaml
    /// </summary>
    public partial class SpaceAndDescriptionsTabControl : UserControl
    {
        public SpaceAndDescriptionsTabControl()
        {
            InitializeComponent();
            if (Defines.Zagga)
                ExtendedDescriptionsTabItem.Visibility = Visibility.Collapsed;
        }

        public void Item_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as DescribableInterval;

            if (item != null)
                item.NavigateToCommand.Execute();
        }
    }
}
