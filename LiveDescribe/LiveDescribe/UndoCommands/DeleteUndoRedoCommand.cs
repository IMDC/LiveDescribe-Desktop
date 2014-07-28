using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.UndoCommands
{
    public class DeleteSpaceUndoRedoCommand : IUndoRedoCommand
    {
        private ObservableCollection<Space> _collection;
        private Space _elementRemoved;

        public DeleteSpaceUndoRedoCommand(ObservableCollection<Space> collection, Space element)
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
            _collection.Add(_elementRemoved);
        }
    }
}
