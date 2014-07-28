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
    public class InsertSpaceUndoRedoCommand : IUndoRedoCommand
    {
        private Space _elementToInsert;
        private ObservableCollection<Space> _collection;

        public InsertSpaceUndoRedoCommand(ObservableCollection<Space> collection, Space element)
        {
            _collection = collection;
            _elementToInsert = element;
        }

        public void Execute()
        {
            _collection.Add(_elementToInsert);
        }

        public void UnExecute()
        {
            _collection.Remove(_elementToInsert);
        }
    }
}
