using LiveDescribe.Interfaces;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Contains information relating to selected canvas items.
    /// </summary>
    public class CanvasMouseSelection
    {
        /// <summary>
        /// Represents no currently selected item. The item property is null.
        /// </summary>
        public static readonly CanvasMouseSelection NoSelection =
            new CanvasMouseSelection(IntervalMouseAction.None, null, 0);

        public CanvasMouseSelection(IntervalMouseAction action, IDescribableInterval item,
            double mouseClickTimeDifference)
        {
            Action = action;
            Item = item;
            MouseClickTimeDifference = mouseClickTimeDifference;
        }

        public IntervalMouseAction Action { private set; get; }

        public IDescribableInterval Item { private set; get; }

        /// <summary>
        /// The difference of the mouse's initial click time minus the interval's start time.
        /// </summary>
        public double MouseClickTimeDifference { private set; get; }
    }
}
