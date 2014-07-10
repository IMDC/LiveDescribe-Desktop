﻿using LiveDescribe.Model;
using LiveDescribe.Utilities;
using System.Windows;
using System.Windows.Input;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for DescriptionControl.xaml
    /// </summary>
    public partial class DescriptionControl : ItemControl
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof (Description), typeof (DescriptionControl));

        private double _originalPositionForDraggingDescription;

        public DescriptionControl()
        {
            InitializeComponent();
        }

        public Description Description
        {
            get { return (Description)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        private void DescriptionGraphic_Loaded(object sender, RoutedEventArgs e)
        {
            Container.CurrentActionState = ItemCanvas.ActionState.None;
        }

        private void DescriptionGraphic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Description.DescriptionMouseDownCommand.Execute(e);
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                _originalPositionForDraggingDescription = e.GetPosition(Container).X;
                DescriptionGraphic.CaptureMouse();
                Container.Cursor = CustomCursors.GrabbingCursor;
                Container.CurrentActionState = ItemCanvas.ActionState.Dragging;
            }
        }

        private void DescriptionGraphic_MouseMove(object sender, MouseEventArgs e)
        {
            Description.DescriptionMouseMoveCommand.Execute(e);
            double xPos = e.GetPosition(Container).X;
            if (DescriptionGraphic.IsMouseCaptured)
                HandleDescriptionMouseCapturedState(xPos);
            else
                HandlerDescriptionNonMouseCapturedState();
        }

        private void DescriptionGraphic_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OnFinishDescriptionActionState();
        }

        private void HandlerDescriptionNonMouseCapturedState()
        {
            Mouse.SetCursor(CustomCursors.GrabCursor);
        }

        private void HandleDescriptionMouseCapturedState(double xPos)
        {
            if (Container.CurrentActionState == ItemCanvas.ActionState.Dragging)
                DragDescription(xPos);
        }

        private void DragDescription(double xPos)
        {
            if (!CanContinueDraggingDescription(xPos))
                return;

            double newPosition = Description.X + (xPos - _originalPositionForDraggingDescription);
            double newPositionMilliseconds = (Container.VideoDuration / Container.Width) * newPosition;
            double lengthOfDescriptionMilliseconds = Description.EndInVideo - Description.StartInVideo;

            //bounds checking when dragging the description
            if (newPositionMilliseconds < 0)
                newPosition = 0;
            else if ((newPositionMilliseconds + lengthOfDescriptionMilliseconds) > Container.VideoDuration)
                newPosition = (Container.Width / Container.VideoDuration) * (Container.VideoDuration - lengthOfDescriptionMilliseconds);

            Description.X = newPosition;
            _originalPositionForDraggingDescription = xPos;
            Description.StartInVideo = (Container.VideoDuration / Container.Width) * (Description.X);
            Description.EndInVideo = Description.StartInVideo + (Description.EndWaveFileTime - Description.StartWaveFileTime);
        }

        private bool CanContinueDraggingDescription(double xPos)
        {
            return (!(xPos < 0 || xPos >= Container.Width));
        }

        private void OnFinishDescriptionActionState()
        {
            DescriptionGraphic.ReleaseMouseCapture();
            Container.CurrentActionState = ItemCanvas.ActionState.None;
            Container.Cursor = Cursors.Arrow;
        }
    }
}
