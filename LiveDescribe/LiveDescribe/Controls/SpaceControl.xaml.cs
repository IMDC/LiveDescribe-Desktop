using LiveDescribe.Model;
using LiveDescribe.Utilities;
using System.Windows;
using System.Windows.Input;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for SpaceControl.xaml
    /// </summary>
    public partial class SpaceControl : ItemControl
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static DependencyProperty SpaceProperty =
            DependencyProperty.Register("Space", typeof(Space), typeof(SpaceControl));

        public const double MinSpaceLengthInMSecs = 333;
        private const int ResizeSpaceOffset = 10;
        private double _originalPositionForDraggingSpace;

        public SpaceControl()
        {
            InitializeComponent();
        }

        public Space Space
        {
            get { return (Space)GetValue(SpaceProperty); }
            set { SetValue(SpaceProperty, value); }
        }

        private void SpaceGraphic_Loaded(object sender, RoutedEventArgs e)
        {
            Container.CurrentActionState = ItemCanvas.ActionState.None;
        }

        private void SpaceGraphic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Space.SpaceMouseDownCommand.Execute(e);
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                double xPos = e.GetPosition(Container).X;
                _originalPositionForDraggingSpace = xPos;
                SpaceGraphic.CaptureMouse();
                SetCursorUponMouseDown(xPos);
            }
        }

        private void SetCursorUponMouseDown(double xPos)
        {
            if (xPos > (Space.X + Space.Width - ResizeSpaceOffset))
            {
                Container.Cursor = Cursors.SizeWE;
                Container.CurrentActionState = ItemCanvas.ActionState.ResizingEndOfItem;
            }
            else if (xPos < (Space.X + ResizeSpaceOffset))
            {
                Container.Cursor = Cursors.SizeWE;
                Container.CurrentActionState = ItemCanvas.ActionState.ResizingBeginningOfItem;
            }
            else
            {
                Container.Cursor = CustomCursors.GrabbingCursor;
                Container.CurrentActionState = ItemCanvas.ActionState.Dragging;
            }
        }

        private void SpaceGraphic_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FinishActionOnSpace();
        }

        private void FinishActionOnSpace()
        {
            SpaceGraphic.ReleaseMouseCapture();
            Container.CurrentActionState = ItemCanvas.ActionState.None;
            Container.Cursor = Cursors.Arrow;
        }

        private void SpaceGraphic_MouseMove(object sender, MouseEventArgs e)
        {
            Space.SpaceMouseMoveCommand.Execute(e);
            double xPos = e.GetPosition(Container).X;

            if (SpaceGraphic.IsMouseCaptured)
                HandleSpaceMouseCapturedStates(xPos);
            else
                HandleSpaceNonMouseCapturedStates(xPos);
        }

        private void HandleSpaceMouseCapturedStates(double xPos)
        {
            if (Container.CurrentActionState == ItemCanvas.ActionState.ResizingEndOfItem)
                ResizeEndOfSpace(xPos);
            else if (Container.CurrentActionState == ItemCanvas.ActionState.ResizingBeginningOfItem)
                ResizeBeginningOfSpace(xPos);
            else if (Container.CurrentActionState == ItemCanvas.ActionState.Dragging)
                DragSpace(xPos);

            SetAppropriateCursorUponMouseCaptured();
        }

        private void HandleSpaceNonMouseCapturedStates(double xPos)
        {
            //Changes cursor if the mouse hovers over the end or the beginning of the space
            if (xPos > (Space.X + Space.Width - ResizeSpaceOffset)) //mouse is over right side of the space
                Mouse.SetCursor(Cursors.SizeWE);
            else if (xPos < (Space.X + ResizeSpaceOffset)) //mouse is over left size of space
                Mouse.SetCursor(Cursors.SizeWE);
            else
                Mouse.SetCursor(CustomCursors.GrabCursor);
        }

        private void ResizeEndOfSpace(double mouseXPosition)
        {
            double newWidth = Space.Width + (mouseXPosition - _originalPositionForDraggingSpace);
            double lengthInMillisecondsNewWidth = (Container.VideoDuration / Container.Width) * newWidth;

            //bounds checking
            if (lengthInMillisecondsNewWidth < MinSpaceLengthInMSecs)
            {
                newWidth = (Container.Width / Container.VideoDuration) * MinSpaceLengthInMSecs;
                //temporary fix, have to make the cursor attached to the end of the space somehow
                FinishActionOnSpace();
            }
            else if ((Space.StartInVideo + lengthInMillisecondsNewWidth) > Container.VideoDuration)
            {
                newWidth = (Container.Width / Container.VideoDuration) * (Container.VideoDuration - Space.StartInVideo);
                //temporary fix, have to make the cursor attached to the end of the space somehow
                FinishActionOnSpace();
            }

            Space.Width = newWidth;
            _originalPositionForDraggingSpace = mouseXPosition;
            Space.EndInVideo = Space.StartInVideo + (Container.VideoDuration / Container.Width) * Space.Width;
        }

        private void ResizeBeginningOfSpace(double mouseXPosition)
        {
            //left side of space
            double newPosition = Space.X + (mouseXPosition - _originalPositionForDraggingSpace);
            double newPositionMilliseconds = (Container.VideoDuration / Container.Width) * newPosition;

            //bounds checking
            if (newPositionMilliseconds < 0)
            {
                newPosition = 0;
                //temporary fix, have to make the cursor attached to the end of the space somehow
                FinishActionOnSpace();
            }
            else if ((Space.EndInVideo - newPositionMilliseconds) < MinSpaceLengthInMSecs)
            {
                newPosition = (Container.Width / Container.VideoDuration) * (Space.EndInVideo - MinSpaceLengthInMSecs);
                //temporary fix, have to make the cursor attached to the end of the space somehow
                FinishActionOnSpace();
            }

            Space.X = newPosition;
            Space.StartInVideo = (Container.VideoDuration / Container.Width) * newPosition;
            Space.Width = (Container.Width / Container.VideoDuration) * (Space.EndInVideo - Space.StartInVideo);

            _originalPositionForDraggingSpace = mouseXPosition;
        }

        private void DragSpace(double mouseXPosition)
        {
            if (!CanContinueDraggingSpace(mouseXPosition))
                return;

            double newPosition = Space.X + (mouseXPosition - _originalPositionForDraggingSpace);
            double newPositionMilliseconds = (Container.VideoDuration / Container.Width) * newPosition;
            double lengthOfSpaceMilliseconds = Space.EndInVideo - Space.StartInVideo;
            //size in pixels of the space
            double size = (Container.Width / Container.VideoDuration) * lengthOfSpaceMilliseconds;

            if (newPositionMilliseconds < 0)
                newPosition = 0;
            else if ((newPositionMilliseconds + lengthOfSpaceMilliseconds) > Container.VideoDuration)
                newPosition = (Container.Width / Container.VideoDuration) * (Container.VideoDuration - lengthOfSpaceMilliseconds);

            Space.X = newPosition;
            _originalPositionForDraggingSpace = mouseXPosition;
            Space.StartInVideo = (Container.VideoDuration / Container.Width) * (Space.X);
            Space.EndInVideo = Space.StartInVideo + (Container.VideoDuration / Container.Width) * size;
        }

        private bool CanContinueDraggingSpace(double xPos)
        {
            return (!(xPos < 0 || xPos >= Container.Width));
        }

        private void SetAppropriateCursorUponMouseCaptured()
        {
            if (Container.Cursor != CustomCursors.GrabbingCursor && Container.CurrentActionState == ItemCanvas.ActionState.Dragging)
                Container.Cursor = CustomCursors.GrabbingCursor;

            if (Container.Cursor != Cursors.SizeWE && (Container.CurrentActionState == ItemCanvas.ActionState.ResizingBeginningOfItem ||
                Container.CurrentActionState == ItemCanvas.ActionState.ResizingEndOfItem))
                Container.Cursor = Cursors.SizeWE;
        }
    }
}
