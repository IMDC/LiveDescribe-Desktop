using System.Collections.ObjectModel;

namespace LiveDescribe.Model
{
    /// <summary>
    /// An observable collection that will only contain a certain amount of items. After the
    /// specified item limit as been reached, the collection will drop the item at the end of the
    /// list. Note: Only use the methods provided below. Other methods are not supported.
    /// </summary>
    /// <typeparam name="T">Type of the items in the collection.</typeparam>
    public class ObservableDropoutCollection<T> : ObservableCollection<T>
    {
        private const int DefaultItemLimit = 10;

        public ObservableDropoutCollection()
        {
            ItemLimit = DefaultItemLimit;
        }

        /// <summary>
        /// How many items this Collection can contain before dropping some.
        /// </summary>
        public int ItemLimit { set; get; }

        /// <summary>
        /// Adds an item to the front of the list. If the item is already contained in that list,
        /// it will be brought to the front. If there are more items than the ItemLimit allows, the
        /// last item in the list will be dropped.
        /// </summary>
        /// <param name="item">The item to insert</param>
        public void AddFirst(T item)
        {
            if (Contains(item))
                Remove(item);
            if (ItemLimit <= Count)
                DropLast();

            InsertItem(0, item);
        }

        private void DropLast()
        {
            while (ItemLimit <= Count)
                RemoveAt(Count - 1);
        }
    }
}
