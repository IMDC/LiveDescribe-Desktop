using LiveDescribe.Model;
using LiveDescribe.Resources;
using System;
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

        public const double Tolerance = 0.0001;

        public static DependencyProperty SpaceProperty =
            DependencyProperty.Register("Space", typeof(Space), typeof(SpaceControl));

        public const double MinSpaceLengthInMSecs = 333;
        private const int ResizeSpaceOffset = 10;
        private double _originalMousePositionForDraggingSpace;

        private double _originalStartInVideo;
        private double _originalEndInVideo;

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

                _originalMousePositionForDraggingSpace = xPos;
                _originalStartInVideo = Space.StartInVideo;
                _originalEndInVideo = Space.EndInVideo;

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
                Container.Cursor = CustomResources.GrabbingCursor;
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

            if (Mouse.LeftButton == MouseButtonState.Released)
                SetupUndoAndRedo();

            Container.CurrentActionState = ItemCanvas.ActionState.None;
            Container.Cursor = Cursors.Arrow;
        }

        private void SetupUndoAndRedo()
        {
            if (Container.CurrentActionState == ItemCanvas.ActionState.Dragging ||
                Container.CurrentActionState == ItemCanvas.ActionState.ResizingBeginningOfItem ||
                Container.CurrentActionState == ItemCanvas.ActionState.ResizingEndOfItem)
            {
                if (!(Math.Abs(_originalEndInVideo - Space.EndInVideo) < Tolerance && Math.Abs(_originalStartInVideo - Space.StartInVideo) < Tolerance))
                {
                    Container.UndoRedoManager.InsertItemForMoveOrResizeUndoRedo(Space, _originalStartInVideo,
                        _originalEndInVideo,
                        Space.StartInVideo, Space.EndInVideo);
                }
            }
        }

        private void SpaceGraphic_MouseMove(object sender, MouseEventArgs e)
        {
            if (Space.LockedInPlace)
                return;

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
                Mouse.SetCursor(CustomResources.GrabCursor);
        }

        private void ResizeEndOfSpace(double mouseXPosition)
        {
            double newWidth = Space.Width + (mouseXPosition - _originalMousePositionForDraggingSpace);
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

            _originalMousePositionForDraggingSpace = mouseXPosition;
            Space.EndInVideo = Space.StartInVideo + (Container.VideoDuration / Container.Width) * newWidth;
        }

        private void ResizeBeginningOfSpace(double mouseXPosition)
        {
            //left side of space
            double newPosition = Space.X + (mouseXPosition - _originalMousePositionForDraggingSpace);
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

            Space.StartInVideo = (Container.VideoDuration / Container.Width) * newPosition;
            _originalMousePositionForDraggingSpace = mouseXPosition;
        }

        private void DragSpace(double mouseXPosition)
        {
            if (!CanContinueDraggingSpace(mouseXPosition))
                return;

            double newPosition = Space.X + (mouseXPosition - _originalMousePositionForDraggingSpace);
            double newPositionMilliseconds = (Container.VideoDuration / Container.Width) * newPosition;
            double lengthOfSpaceMilliseconds = Space.EndInVideo - Space.StartInVideo;
            //size in pixels of the space
            double size = (Container.Width / Container.VideoDuration) * lengthOfSpaceMilliseconds;

            if (newPositionMilliseconds < 0)
                newPosition = 0;
            else if ((newPositionMilliseconds + lengthOfSpaceMilliseconds) > Container.VideoDuration)
                newPosition = (Container.Width / Container.VideoDuration) * (Container.VideoDuration - lengthOfSpaceMilliseconds);

            _originalMousePositionForDraggingSpace = mouseXPosition;
            Space.StartInVideo = (Container.VideoDuration / Container.Width) * (newPosition);
            Space.EndInVideo = Space.StartInVideo + (Container.VideoDuration / Container.Width) * size;
        }

        private bool CanContinueDraggingSpace(double xPos)
        {
            return (!(xPos < 0 || xPos >= Container.Width));
        }

        private void SetAppropriateCursorUponMouseCaptured()
        {
            if (Container.Cursor != CustomResources.GrabbingCursor && Container.CurrentActionState == ItemCanvas.ActionState.Dragging)
                Container.Cursor = CustomResources.GrabbingCursor;

            if (Container.Cursor != Cursors.SizeWE && (Container.CurrentActionState == ItemCanvas.ActionState.ResizingBeginningOfItem ||
                Container.CurrentActionState == ItemCanvas.ActionState.ResizingEndOfItem))
                Container.Cursor = Cursors.SizeWE;
        }
    }
}
