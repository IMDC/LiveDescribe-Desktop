namespace LiveDescribe.Controls.Canvases
{
    public enum IntervalMouseAction
    {
        /// <summary>
        /// When the mouse has no interval selected.
        /// </summary>
        None,
        /// <summary>
        /// When the mouse has selected an interval but is not currently modifying it.
        /// </summary>
        ItemSelected,
        /// <summary>
        /// When the mouse has selected an interval and is dragging it along the timeline.
        /// </summary>
        Dragging,
        /// <summary>
        /// When the mouse has selected an interval and is changing its endtime.
        /// </summary>
        ChangeEndTime,
        /// <summary>
        /// When the mouse has selected an interval and is changing its start time.
        /// </summary>
        ChangeStartTime
    };
}