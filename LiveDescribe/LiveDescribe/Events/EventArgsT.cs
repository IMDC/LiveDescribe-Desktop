using System;

namespace LiveDescribe.Events
{
    /// <summary>
    /// A Generic EventArgs object that is meant to hold a single value used in events.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data to pass to the event handler in this EventArgs instance.
    /// </typeparam>
    public class EventArgs<T> : EventArgs
    {
        public static implicit operator EventArgs<T>(T value)
        {
            return new EventArgs<T>(value);
        }

        /// <summary>
        /// The Value contained in this EventArgs
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Initializes an instance of the EventArgs class.
        /// </summary>
        /// <param name="value">The value to pass to the event handler.</param>
        public EventArgs(T value)
        {
            Value = value;
        }
    }
}
