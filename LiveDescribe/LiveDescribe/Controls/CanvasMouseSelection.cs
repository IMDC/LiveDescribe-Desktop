using LiveDescribe.Model;

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
            new CanvasMouseSelection(IntervalMouseAction.None, null);

        public CanvasMouseSelection(IntervalMouseAction action, Space item)
        {
            Action = action;
            Item = item;
        }

        public IntervalMouseAction Action { private set; get; }

        public Space Item { private set; get; }
    }
}
