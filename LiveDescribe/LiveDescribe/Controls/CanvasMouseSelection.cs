using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using System;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Contains information relating to selected canvas items.
    /// </summary>
    public class CanvasMouseSelection
    {
        private const double IntervalTimeDifferenceEpsilon = 0.001;

        /// <summary>
        /// Represents no currently selected item. The item property is null.
        /// </summary>
        public static readonly CanvasMouseSelection NoSelection =
            new CanvasMouseSelection(IntervalMouseAction.None);

        public CanvasMouseSelection(IntervalMouseAction action)
        {
            Action = action;
        }

        public CanvasMouseSelection(IntervalMouseAction action, IDescribableInterval item,
            double mouseClickTimeDifference)
            : this(action)
        {
            Item = item;
            MouseClickTimeDifference = mouseClickTimeDifference;

            OriginalStartInVideo = item.StartInVideo;
            OriginalEndInVideo = item.EndInVideo;
        }

        /// <summary>
        /// The original start time of the space before it is modified.
        /// </summary>
        public double OriginalStartInVideo { private set; get; }

        /// <summary>
        /// The original end time of the space before it is modified.
        /// </summary>
        public double OriginalEndInVideo { private set; get; }

        /// <summary>
        /// The action being performed on this mouse selection.
        /// </summary>
        public IntervalMouseAction Action { private set; get; }

        /// <summary>
        /// The interval being modified.
        /// </summary>
        public IDescribableInterval Item { private set; get; }

        /// <summary>
        /// The difference of the mouse's initial click time minus the interval's start time.
        /// </summary>
        public double MouseClickTimeDifference { private set; get; }

        public bool HasItemBeenModified()
        {
            return Math.Abs(OriginalStartInVideo - Item.StartInVideo) > IntervalTimeDifferenceEpsilon
                || Math.Abs(OriginalEndInVideo - Item.EndInVideo) > IntervalTimeDifferenceEpsilon;
        }

        public void AddChangesTo(UndoRedoManager manager)
        {
            manager.InsertItemForMoveOrResizeUndoRedo(Item, OriginalStartInVideo, OriginalEndInVideo,
                Item.StartInVideo, Item.EndInVideo);
        }
    }
}
