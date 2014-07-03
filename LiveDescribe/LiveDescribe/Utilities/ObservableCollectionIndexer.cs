using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Interfaces;

namespace LiveDescribe.Utilities
{
    /// <summary>
    /// Automatically indexes the items in a given ObservableCollection. The collection has to be
    /// of a type that implements the IListIndexable interface. Indecies start from 1.
    /// </summary>
    /// <typeparam name="T">The type of the Observable Collection</typeparam>
    public class ObservableCollectionIndexer<T>
    {
        private readonly ObservableCollection<T> _collection;

        public ObservableCollectionIndexer(ObservableCollection<T> collection)
        {
            if(IsInvalidType())
                throw new ArgumentException("Collection does not implement IListIndexible");

            _collection = collection;
            _collection.CollectionChanged += CollectionChangedListener;
        }

        private bool IsInvalidType()
        {
            return !typeof (IListIndexable).IsAssignableFrom(typeof(T));
        }

        public void CollectionChangedListener(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IndexCollectionItems(args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Move:
                    IndexCollectionItems(args.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IndexCollectionItems();
                    break;
            }
        }

        /// <summary>
        /// Sets the indices of all the items in this collection from the starting index. Indices
        /// are 1-indexed.
        /// </summary>
        /// <param name="startingIndex"></param>
        private void IndexCollectionItems(int startingIndex = 0)
        {
            for (int i = startingIndex; i < _collection.Count; i++)
            {
                var item = (IListIndexable) _collection[i];
                item.Index = i + 1;
            }
        }
    }
}
