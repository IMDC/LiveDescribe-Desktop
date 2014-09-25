using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveDescribe.Extensions;
using LiveDescribe.Model;
using LiveDescribe.Properties;

namespace LiveDescribe.Controls.UserControls
{
    /// <summary>
    /// Interaction logic for IntervalInfoListControl.xaml
    /// </summary>
    public partial class IntervalInfoListControl : UserControl
    {
        public IntervalInfoListControl()
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

        private void SpacesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
