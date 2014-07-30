using System;
using System.Collections.ObjectModel;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;

namespace LiveDescribe.HistoryItems
{
    public class InsertSpaceHistoryItem : IHistoryItem
    {
        private readonly Space _elementToInsert;
        private readonly ObservableCollection<Space> _collection;

        public InsertSpaceHistoryItem(ObservableCollection<Space> collection, Space element)
        {
            _collection = collection;
            _elementToInsert = element;
        }

        public void Execute()
        {
            _collection.Insert(Math.Max(0, _elementToInsert.Index - 1), _elementToInsert);
        }

        public void UnExecute()
        {
            _collection.Remove(_elementToInsert);
        }
    }
}
