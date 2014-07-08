﻿using System.Windows;
using System.Windows.Controls;

namespace LiveDescribe.Controls
{
    public class ItemCanvas : Canvas
    {
        public enum ActionState { None, Dragging, ResizingEndOfItem, ResizingBeginningOfItem };

        public static readonly DependencyProperty VideoDurationProperty =
          DependencyProperty.Register("VideoDuration", typeof(double), typeof(SpaceControl));

        public ItemCanvas() { }

        public double VideoDuration
        {
            get { return (double)GetValue(VideoDurationProperty); }
            set { SetValue(VideoDurationProperty, value); }
        }

        public ActionState CurrentActionState { get; set; }
    }
}
