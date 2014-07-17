using System;
using System.Windows.Controls;
using System.Windows.Input;
using LiveDescribe.Extensions;
using LiveDescribe.Model;

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
        }

        public void Item_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem) sender).Content;
            if (item is Space)
                ((Space)item).GoToThisSpaceCommand.Execute();
            else if (item is Description)
                ((Description)item).GoToThisDescriptionCommand.Execute();
        }
    }
}
