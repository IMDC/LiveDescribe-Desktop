using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LiveDescribe.Controls
{
    public partial class ItemControl : UserControl
    {
        public static readonly DependencyProperty ContainerProperty =
           DependencyProperty.Register("Container", typeof(ItemCanvas), typeof(ItemControl));

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double), typeof(ItemControl));

        public ItemCanvas Container
        {
            get { return (ItemCanvas)GetValue(ContainerProperty); }
            set { SetValue(ContainerProperty, value); }
        }

        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }
    }
}
