using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using LiveDescribe.UndoCommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LiveDescribe.Managers
{
    public class UndoRedoManager
    {
        private Stack<IUndoRedoCommand> _undoStack;
        private Stack<IUndoRedoCommand> _redoStack;

        public UndoRedoManager() 
        {
            _undoStack = new Stack<IUndoRedoCommand>();
            _redoStack = new Stack<IUndoRedoCommand>();
            RedoCommand = new RelayCommand(Redo, CanRedo);
            UndoCommand = new RelayCommand(Undo, CanUndo);
        }

        public ICommand RedoCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }

        public void Redo()
        {
            var cmd = _redoStack.Pop();
            cmd.Execute();
            _undoStack.Push(cmd);
            CommandManager.InvalidateRequerySuggested();
        }

        public void Undo()
        {
            var cmd = _undoStack.Pop();
            cmd.UnExecute();
            _redoStack.Push(cmd);
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanRedo()
        {
            return (_redoStack.Count != 0) ? true : false;
        }

        public bool CanUndo()
        {
            return (_undoStack.Count != 0) ? true : false;
        }

        public void InsertSpaceForInsertInCollection(ObservableCollection<Space> collection, Space element)
        {
            var cmd = new InsertSpaceUndoRedoCommand(collection, element);
            _undoStack.Push(cmd); _redoStack.Clear();
        }

        public void InsertSpaceForDeletion(ObservableCollection<Space> collection, Space element)
        {
            var cmd = new DeleteSpaceUndoRedoCommand(collection, element);
            _undoStack.Push(cmd); _redoStack.Clear();
        }
    }
}
