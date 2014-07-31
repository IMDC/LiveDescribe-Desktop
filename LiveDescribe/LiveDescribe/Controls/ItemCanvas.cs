using System.Windows;
using System.Windows.Controls;
using LiveDescribe.Managers;

namespace LiveDescribe.Controls
{
    public class ItemCanvas : Canvas
    {
        public enum ActionState { None, Dragging, ResizingEndOfItem, ResizingBeginningOfItem };

        public static readonly DependencyProperty VideoDurationProperty =
          DependencyProperty.Register("VideoDuration", typeof(double), typeof(ItemCanvas));

        public double VideoDuration
        {
            get { return (double)GetValue(VideoDurationProperty); }
            set { SetValue(VideoDurationProperty, value); }
        }

        public ActionState CurrentActionState { get; set; }

        public UndoRedoManager UndoRedoManager { get; set; }
    }
}
