using System.Collections.ObjectModel;

namespace LiveDescribe.Model
{
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
        /// <param name="item"></param>
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
