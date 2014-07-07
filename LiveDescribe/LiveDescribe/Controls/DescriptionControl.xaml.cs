﻿using System;
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
using LiveDescribe.Utilities;
using LiveDescribe.Model;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for DescriptionControl.xaml
    /// </summary>
    public partial class DescriptionControl : ItemControl
    {

        private Description _description;
        private double _originalPositionForDraggingDescription;

        public DescriptionControl()
        {
            InitializeComponent();
        }

        private void DescriptionGraphic_Loaded(object sender, RoutedEventArgs e)
        {
            _description = (Description)DataContext;
        }

        private void DescriptionGraphic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _description.DescriptionMouseDownCommand.Execute(e);
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
            _description.DescriptionMouseMoveCommand.Execute(e);
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

            double newPosition = _description.X + (xPos - _originalPositionForDraggingDescription);
            double newPositionMilliseconds = (Container.VideoDuration / Container.Width) * newPosition;
            double lengthOfDescriptionMilliseconds = _description.EndInVideo - _description.StartInVideo;

            //bounds checking when dragging the description
            if (newPositionMilliseconds < 0)
                newPosition = 0;
            else if ((newPositionMilliseconds + lengthOfDescriptionMilliseconds) > Container.VideoDuration)
                newPosition = (Container.Width / Container.VideoDuration) * (Container.VideoDuration - lengthOfDescriptionMilliseconds);

            _description.X = newPosition;
            _originalPositionForDraggingDescription = xPos;
            _description.StartInVideo = (Container.VideoDuration / Container.Width) * (_description.X);
            _description.EndInVideo = _description.StartInVideo + (_description.EndWaveFileTime - _description.StartWaveFileTime);
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
