using System.Windows;
using System.Windows.Controls;

namespace LiveDescribe.Controls
{
    public class ItemControl : UserControl
    {
        public static readonly DependencyProperty ContainerProperty =
           DependencyProperty.Register("Container", typeof(ItemCanvas), typeof(ItemControl));

        public ItemCanvas Container
        {
            get { return (ItemCanvas)GetValue(ContainerProperty); }
            set { SetValue(ContainerProperty, value); }
        }
    }
}
