using LiveDescribe.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LiveDescribe.Utilities
{
    /// <summary>
    /// Automatically indexes the items in a given ObservableCollection. The collection has to be
    /// of a type that implements the IListIndexable interface. Indecies start from 1.
    /// </summary>
    /// <typeparam name="T">The type of the Observable Collection</typeparam>
    public class ObservableCollectionIndexer<T>
    {
        private ObservableCollection<T> _collection;

        public ObservableCollectionIndexer(ObservableCollection<T> collection)
        {
            if (IsInvalidType())
                throw new ArgumentException("Collection does not implement IListIndexible");

            Collection = collection;
        }

        public ObservableCollection<T> Collection
        {
            set
            {
                CollectionCleanup();
                _collection = value;
                CollectionSetup();
            }
            get { return _collection; }
        }

        private void CollectionCleanup()
        {
            if (Collection != null)
                Collection.CollectionChanged -= CollectionChangedListener;
        }

        private void CollectionSetup()
        {
            if (Collection != null)
                Collection.CollectionChanged += CollectionChangedListener;
        }

        private static bool IsInvalidType()
        {
            return !typeof(IListIndexable).IsAssignableFrom(typeof(T));
        }

        public void CollectionChangedListener(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IndexItems(args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Move:
                    IndexItems(args.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IndexItems();
                    break;
            }
        }

        /// <summary>
        /// Sets the indices of all the items in this collection from the starting index. Indices
        /// are 1-indexed.
        /// </summary>
        /// <param name="startingIndex"></param>
        private void IndexItems(int startingIndex = 0)
        {
            for (int i = startingIndex; i < _collection.Count; i++)
            {
                var item = (IListIndexable)_collection[i];
                item.Index = i + 1;
            }
        }
    }
}
