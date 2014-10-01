using LiveDescribe.Extensions;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiveDescribe.Controls.Canvases
{
    public abstract class GeometryImageCanvas : Canvas
    {
        #region Constants
        /// <summary>
        /// The pen used to draw the borders of descriptions, spaces, etc.
        /// </summary>
        protected static readonly Pen LinePen = PenFactory.LinePen(Brushes.Black);

        /// <summary>
        /// The distance away from the beginning or ending of an interval in pixels that the user
        /// has to click on to be able to move the caption.
        /// </summary>
        protected const double SelectionPixelWidth = 10;

        /// <summary>
        /// Smallest amound of time that an interval can be.
        /// </summary>
        protected const double MinIntervalDurationMsec = 300;
        #endregion

        #region Fields and Properties

        /// <summary>
        /// The image that displays the currently selected image. This is implemented as a
        /// reference variable and not a property so that it can be passed to the AddImageToCanvas
        /// method using the ref keyword.
        /// </summary>
        protected Image SelectedImage;

        /// <summary>
        /// The leftmost pixel of the canvas is visible to the user.
        /// </summary>
        public double VisibleX { get; set; }

        /// <summary>
        /// How many pixels wide the viewable area of the canvas is.
        /// </summary>
        public double VisibleWidth { get; set; }

        /// <summary>
        /// The length of the video in milliseconds.
        /// </summary>
        public double VideoDurationMsec { get; set; }

        /// <summary>
        /// The item, if any, currently selected by the mouse.
        /// </summary>
        protected CanvasMouseSelection MouseSelection { get; set; }

        /// <summary>
        /// The brush to paint the currently selected item with.
        /// </summary>
        protected Brush SelectedItemBrush { get; set; }

        public IntervalMouseAction MouseAction
        {
            get { return MouseSelection.Action; }
        }

        #endregion

        #region Drawing Methods
        /// <summary>
        /// Draws all the contents of the canvas.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Draws the selectedItem image based off of mouse selection. Call this method to only
        /// draw the currently selected interval.
        /// </summary>
        public virtual void DrawMouseSelection()
        {
            if (MouseSelection.Action == IntervalMouseAction.None)
                return;

            Children.Remove(SelectedImage);

            var selectedGroup = new GeometryGroup();
            selectedGroup.Children.Add(new RectangleGeometry(new Rect
            {
                X = MouseSelection.Item.X,
                Y = MouseSelection.Item.Y,
                Width = MouseSelection.Item.Width,
                Height = MouseSelection.Item.Height,
            }));

            SelectedImage = selectedGroup.CreateImage(SelectedItemBrush, LinePen);

            Children.Add(SelectedImage);

            SetLeft(SelectedImage, selectedGroup.Children[0].Bounds.X);
            SetTop(SelectedImage, 0);
        }

        /// <summary>
        /// Draws a DrawingVisual object to the canvas by setting it as the given image's source.
        /// In order to be seen, the image must already be a child of the canvas that is being
        /// displayed to.
        /// </summary>
        /// <param name="image">Image to set the source of.</param>
        /// <param name="drawingVisual">Drawing visual to display.</param>
        /// <param name="topPos">The y position that the image will be set to.</param>
        protected void DisplayVisualOnCanvas(Image image, DrawingVisual drawingVisual, double topPos = 0)
        {
            if (drawingVisual == null || drawingVisual.Drawing == null || drawingVisual.Drawing.Children.Count < 1)
            {
                image.Source = null;
                return;
            }

            drawingVisual.Drawing.Freeze();

            var drawingImage = new DrawingImage(drawingVisual.Drawing);
            drawingImage.Freeze();

            image.Source = drawingImage;

            //The Image has to be set to the smallest X value of the visible spaces.
            double minX = drawingVisual.Drawing.Children[0].Bounds.X;
            for (int i = 1; i < drawingVisual.Drawing.Children.Count; i++)
            {
                minX = Math.Min(minX, drawingVisual.Drawing.Children[i].Bounds.X);
            }

            SetLeft(image, minX);
            SetTop(image, topPos);
        }

        /// <summary>
        /// Tests whether the interval would be visible on the current canvas or not. It will be
        /// visible IFF the interval either starts or ends in the visible area, or covers the
        /// entire visible area.
        /// </summary>
        /// <param name="interval">The interval to test.</param>
        /// <param name="visibleBeginMsec">The beginning of the window in milliseconds.</param>
        /// <param name="visibleEndMsec">The end of the window in milliseconds.</param>
        /// <returns>Whether the interval is visible or not.</returns>
        protected bool IsIntervalVisible(IDescribableInterval interval, double visibleBeginMsec, double visibleEndMsec)
        {
            return (visibleBeginMsec <= interval.StartInVideo && interval.StartInVideo <= visibleEndMsec)
                || (visibleBeginMsec <= interval.EndInVideo && interval.EndInVideo <= visibleEndMsec)
                || (interval.StartInVideo <= visibleBeginMsec && visibleEndMsec <= interval.EndInVideo);
        }

        /// <summary>
        /// Sets the brushes based off of ColourScheme settings. Intended to be called when the
        /// Colourscheme changes.
        /// </summary>
        protected abstract void SetBrushes();

        /// <summary>
        /// Turns an X-Coordinate into the time in milliseconds that it represents on the canvas.
        /// </summary>
        /// <param name="x">The value to convert.</param>
        /// <returns>The resulting time.</returns>
        protected double XPosToMilliseconds(double x)
        {
            return (VideoDurationMsec / (Width)) * x;
        }

        public void SetVisibleBoundaries(double visibleX, double visibleWidth)
        {
            VisibleX = visibleX;
            VisibleWidth = visibleWidth;
        }
        #endregion

        /// <summary>
        /// Returns whether the given value is inclusively between two boundaries.
        /// </summary>
        protected bool IsBetweenBounds(double lowerBound, double value, double upperBound)
        {
            return lowerBound <= value && value <= upperBound;
        }

        /// <summary>
        /// Ensures that the given value is bounded within the given lower and upper bounds.
        /// </summary>
        /// <param name="lowerBound">The lowest number that the value can be.</param>
        /// <param name="value">The value to bounds check.</param>
        /// <param name="upperBound">The highest number that the value can be.</param>
        /// <returns>The value bounded between the two bounds.</returns>
        public double BoundBetween(double lowerBound, double value, double upperBound)
        {
            return Math.Min(upperBound, Math.Max(lowerBound, value));
        }


        public void ClearMouseSelection()
        {
            if (MouseSelection.Action != IntervalMouseAction.None)
            {
                MouseSelection.Item.IsSelected = false;
                MouseSelection = CanvasMouseSelection.NoSelection;
            }
        }

        protected virtual void CollectionChanged_TrackPropertyListeners(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var notifier = item as INotifyPropertyChanged;

                    if (notifier != null)
                        notifier.PropertyChanged += ObservableCollectionElement_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var notifier = item as INotifyPropertyChanged;

                    if (notifier != null)
                        notifier.PropertyChanged -= ObservableCollectionElement_PropertyChanged;
                }
            }
        }

        protected virtual void ObservableCollectionElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StartInVideo"
                || e.PropertyName == "EndInVideo"
                || e.PropertyName == "SetStartAndEndInVideo")
                Draw();
        }
    }
}