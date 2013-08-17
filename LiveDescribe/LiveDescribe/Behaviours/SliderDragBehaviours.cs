using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LiveDescribe.Behaviours
{
    public static class SliderDragBehaviours
    {
        public static readonly DependencyProperty DragStartedCommandProperty =
            DependencyProperty.RegisterAttached("DragStartedCommand", typeof(ICommand), typeof(SliderDragBehaviours),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(DragStarted)));

        private static void DragStarted(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = (Slider)d;
            var thumb = GetThumbFromSlider(slider);

            thumb.DragStarted += thumb_DragStarted;
        }

        private static void thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            element.Dispatcher.Invoke(() =>
            {
                var command = GetDragStartedCommand(element);
                var slider = FindParentControl<Slider>(element) as Slider;
                command.Execute(slider.Value);
            });
        }

        public static void SetDragStartedCommand(UIElement element, ICommand value)
        {
            element.SetValue(DragStartedCommandProperty, value);
        }

        public static ICommand GetDragStartedCommand(FrameworkElement element)
        {
            var slider = FindParentControl<Slider>(element);
            return (ICommand)slider.GetValue(DragStartedCommandProperty);
        }

        private static Thumb GetThumbFromSlider(Slider slider)
        {
            var track = slider.Template.FindName("PART_Track", slider) as Track;
            return track == null ? null : track.Thumb;
        }

        private static DependencyObject FindParentControl<T>(DependencyObject control)
        {
            var parent = VisualTreeHelper.GetParent(control);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent;
        }
    }
}
