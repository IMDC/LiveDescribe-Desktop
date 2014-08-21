using LiveDescribe.Managers;
using System.Windows;
using System.Windows.Controls;

namespace LiveDescribe.Controls
{
    public class ItemCanvas : Canvas
    {
        public static readonly DependencyProperty VideoDurationProperty =
          DependencyProperty.Register("VideoDuration", typeof(double), typeof(ItemCanvas));

        public double VideoDuration
        {
            get { return (double)GetValue(VideoDurationProperty); }
            set { SetValue(VideoDurationProperty, value); }
        }

        public IntervalMouseAction CurrentIntervalMouseAction { get; set; }

        public UndoRedoManager UndoRedoManager { get; set; }
    }
}
