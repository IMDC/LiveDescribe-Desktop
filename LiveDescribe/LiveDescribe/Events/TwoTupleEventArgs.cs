using System;

namespace LiveDescribe.Events
{
    /// <summary>
    /// An event args class that can hold two generic items.
    /// </summary>
    /// <typeparam name="T1">The type of Item1.</typeparam>
    /// <typeparam name="T2">The type of Item2.</typeparam>
    public class TwoTupleEventArgs<T1, T2> : EventArgs
    {
        public TwoTupleEventArgs(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { private set; get; }
        public T2 Item2 { private set; get; }
    }
}
