using System;
using System.Collections.ObjectModel;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;

namespace LiveDescribe.HistoryItems
{
    public class DeleteSpaceHistoryItem : IHistoryItem
    {
        private readonly ObservableCollection<Space> _collection;
        private readonly Space _elementRemoved;

        public DeleteSpaceHistoryItem(ObservableCollection<Space> collection, Space element)
        {
            _collection = collection;
            _elementRemoved = element;
        }

        public void Execute()
        {
            _collection.Remove(_elementRemoved);
        }

        public void UnExecute()
        {
            _collection.Insert(Math.Max(0, _elementRemoved.Index - 1), _elementRemoved);
        }
    }
}
