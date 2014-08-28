using LiveDescribe.Interfaces;
using System;
using System.Windows.Controls;

namespace LiveDescribe.Controls
{
    public abstract class GeometryImageCanvas : Canvas
    {
        /// <summary>
        /// The distance away from the beginning or ending of an interval in pixels that the user
        /// has to click on to be able to move the caption.
        /// </summary>
        protected const double SelectionPixelWidth = 10;

        /// <summary>
        /// Smallest amound of time that an interval can be.
        /// </summary>
        protected const double MinIntervalDurationMsec = 300;

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
        /// Draws all the contents of the canvas.
        /// </summary>
        public abstract void Draw();

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
    }
}