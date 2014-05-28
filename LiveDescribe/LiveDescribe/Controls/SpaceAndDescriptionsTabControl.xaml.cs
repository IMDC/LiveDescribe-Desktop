using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using LiveDescribe.View_Model;

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
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (DescriptionsTabItem.IsSelected)
                {
                    ExtendedDescriptionsListView.SelectedItem = null;
                    SpacesListView.SelectedItem = null;
                }
                else if (SpacesTabItem.IsSelected)
                {
                    ExtendedDescriptionsListView.SelectedItem = null;
                    DescriptionsListView.SelectedItem = null;
                }
                else if (ExtendedDescriptionsTabItem.IsSelected)
                {
                    SpacesListView.SelectedItem = null;
                    DescriptionsListView.SelectedItem = null;
                }
            }
        }
    }
}
