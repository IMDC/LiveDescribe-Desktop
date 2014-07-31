using GalaSoft.MvvmLight.Command;
using LiveDescribe.HistoryItems;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
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
        private readonly Stack<IHistoryItem> _undoStack;
        private readonly Stack<IHistoryItem> _redoStack;

        public UndoRedoManager() 
        {
            _undoStack = new Stack<IHistoryItem>();
            _redoStack = new Stack<IHistoryItem>();
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
            return (_redoStack.Count != 0);
        }

        public bool CanUndo()
        {
            return (_undoStack.Count != 0);
        }

        public void InsertSpaceForInsertUndoRedo(ObservableCollection<Space> collection, Space element)
        {
            var cmd = new InsertSpaceHistoryItem(collection, element);
            _undoStack.Push(cmd); _redoStack.Clear();
        }

        public void InsertSpaceForDeleteUndoRedo(ObservableCollection<Space> collection, Space element)
        {
            var cmd = new DeleteSpaceHistoryItem(collection, element);
            _undoStack.Push(cmd); _redoStack.Clear();
        }

        public void InsertDescriptionForDeleteUndoRedo(ObservableCollection<Description> allDescriptions, 
            ObservableCollection<Description> descriptions, Description element)
        {
            var cmd = new DeleteDescriptionHistoryItem(allDescriptions, descriptions, element);
            _undoStack.Push(cmd); _redoStack.Clear();
        }

        public void InsertDescriptionForInsertUndoRedo(ObservableCollection<Description> allDescriptions,
            ObservableCollection<Description> descriptions, Description element)
        {
            var cmd = new InsertDescriptionHistoryItem(allDescriptions, descriptions, element);
            _undoStack.Push(cmd); _redoStack.Clear();
        }

        public void InsertItemForMoveOrResizeUndoRedo(IDescribableInterval item, double originalStartInVideo, double originalEndInVideo,
            double newStartInVideo, double newEndInVideo)
        {
            var cmd = new MoveOrResizeHistoryItem(item, originalStartInVideo, originalEndInVideo,
                newStartInVideo, newEndInVideo);
            _undoStack.Push(cmd); _redoStack.Clear();
        }
    }
}
