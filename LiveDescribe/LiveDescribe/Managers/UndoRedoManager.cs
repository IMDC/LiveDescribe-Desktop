using GalaSoft.MvvmLight.Command;
using LiveDescribe.HistoryItems;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.Managers
{
    public class UndoRedoManager
    {
        private readonly List<IHistoryItem> _undoStack;
        private readonly List<IHistoryItem> _redoStack;

        public UndoRedoManager() 
        {
            _undoStack = new List<IHistoryItem>();
            _redoStack = new List<IHistoryItem>();
            RedoCommand = new RelayCommand(Redo, CanRedo);
            UndoCommand = new RelayCommand(Undo, CanUndo);
        }

        public ICommand RedoCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }

        public void Redo()
        {
            var cmd = Pop(_redoStack);
            cmd.Execute();
            Push(_undoStack, cmd);
            CommandManager.InvalidateRequerySuggested();
        }

        public void Undo()
        {
            var cmd = Pop(_undoStack);
            cmd.UnExecute();
            Push(_redoStack, cmd);
            CommandManager.InvalidateRequerySuggested();
        }

        private IHistoryItem Pop(List<IHistoryItem> items)
        {
            var lastIndex = items.Count - 1;
            var item = items[lastIndex];
            items.RemoveAt(lastIndex);
            return item;
        }

        private void Push(List<IHistoryItem> items, IHistoryItem item)
        {
            items.Add(item);
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
            Push(_undoStack, cmd); _redoStack.Clear();
        }

        public void InsertSpaceForDeleteUndoRedo(ObservableCollection<Space> collection, Space element)
        {
            var cmd = new DeleteSpaceHistoryItem(collection, element);
            Push(_undoStack, cmd); _redoStack.Clear();
        }

        public void InsertDescriptionForDeleteUndoRedo(ObservableCollection<Description> allDescriptions, 
            ObservableCollection<Description> descriptions, Description element)
        {
            var cmd = new DeleteDescriptionHistoryItem(allDescriptions, descriptions, element);
            Push(_undoStack, cmd); _redoStack.Clear();
        }

        public void InsertDescriptionForInsertUndoRedo(ObservableCollection<Description> allDescriptions,
            ObservableCollection<Description> descriptions, Description element)
        {
            var cmd = new InsertDescriptionHistoryItem(allDescriptions, descriptions, element);
            Push(_undoStack, cmd); _redoStack.Clear();
        }

        public void InsertItemForMoveOrResizeUndoRedo(IDescribableInterval item, double originalStartInVideo, double originalEndInVideo,
            double newStartInVideo, double newEndInVideo)
        {
            var cmd = new MoveOrResizeHistoryItem(item, originalStartInVideo, originalEndInVideo,
                newStartInVideo, newEndInVideo);
            Push(_undoStack, cmd); _redoStack.Clear();
        }
    }
}
