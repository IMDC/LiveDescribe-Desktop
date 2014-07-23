using LiveDescribe.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LiveDescribe.Utilities
{
    /// <summary>
    /// Automatically indexes the items in a given ObservableCollection. The collection has to be
    /// of a type that implements the IListIndexable interface. Indecies start from 1.
    /// </summary>
    /// <typeparam name="T">The type of the Observable Collection</typeparam>
    public static class ObservableCollectionIndexer<T> where T : IListIndexable
    {
        public static void CollectionChangedListener(object sender, NotifyCollectionChangedEventArgs args)
        {
            var collection = (ObservableCollection<T>)sender;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IndexItems(collection, args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Move:
                    IndexItems(collection, args.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IndexItems(collection);
                    break;
            }
        }

        /// <summary>
        /// Sets the indices of all the items in this collection from the starting index. Indices
        /// are 1-indexed.
        /// </summary>
        /// <param name="collection">The ObservableCollection to index.</param>
        /// <param name="startingIndex">Index to start at.</param>
        private static void IndexItems(ObservableCollection<T> collection, int startingIndex = 0)
        {
            for (int i = startingIndex; i < collection.Count; i++)
            {
                var item = collection[i];
                item.Index = i + 1;
            }
        }
    }
}
