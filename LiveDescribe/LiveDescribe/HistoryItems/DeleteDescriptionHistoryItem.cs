using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;

namespace LiveDescribe.HistoryItems
{
    public class DeleteDescriptionHistoryItem : IHistoryItem
    {

        private readonly ObservableCollection<Description> _allDescriptions;
        private readonly ObservableCollection<Description> _descriptions;
        private readonly Description _element;

        public DeleteDescriptionHistoryItem(ObservableCollection<Description> allDescriptions,
            ObservableCollection<Description> descriptions, Description element)
        {
            _allDescriptions = allDescriptions;
            _descriptions = descriptions;
            _element = element;
        }

        public void Execute()
        {
            _allDescriptions.Remove(_element);
            _descriptions.Remove(_element);
        }

        public void UnExecute()
        {
            _descriptions.Insert(Math.Max(0, _element.Index - 1), _element);
            _allDescriptions.Add(_element);
        }
    }
}
