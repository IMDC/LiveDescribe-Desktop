using LiveDescribe.ViewModel;
using LiveDescribe.Extensions;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for MarkingSpacesControl.xaml
    /// </summary>
    public partial class MarkingSpacesControl : UserControl
    {
        private const double Tolerance = 0.0001;

        public MarkingSpacesControl()
        {
            InitializeComponent();

            StartInVideoTextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo,
                                               UndoCommand, CanUndoCommand));
            StartInVideoTextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo,
                                                           RedoCommand, CanRedoCommand));

            EndInVideoTextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo,
                                               UndoCommand, CanUndoCommand));
            EndInVideoTextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo,
                                                           RedoCommand, CanRedoCommand));
        }

        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            var viewmodel = DataContext as MarkingSpacesControlViewModel;
            
            if (e.Key == Key.Return)
            {
                if (viewmodel == null) return;
                var undoRedoManager = viewmodel.UndoRedoManager;
                double originalStartInVideo = viewmodel.SelectedSpace_StartInVideo;
                double originalEndInVideo = viewmodel.SelectedSpace_EndInVideo;

                var textBox = (TextBox)sender;
                BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                if (be != null)
                    be.UpdateSource();

                if (!(Math.Abs(originalEndInVideo - viewmodel.SelectedSpace_EndInVideo) < Tolerance &&
                      Math.Abs(originalStartInVideo - viewmodel.SelectedSpace_StartInVideo) < Tolerance))
                {
                    Console.WriteLine(originalEndInVideo);
                    undoRedoManager.InsertItemForMoveOrResizeUndoRedo(viewmodel.SelectedSpace, originalStartInVideo, originalEndInVideo,
                        viewmodel.SelectedSpace_StartInVideo, viewmodel.SelectedSpace_EndInVideo);
                }
            }
        }

        private void CanRedoCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var viewmodel = DataContext as MarkingSpacesControlViewModel;
            if (viewmodel != null) e.CanExecute = viewmodel.UndoRedoManager.CanRedo();
            e.Handled = true;
        }

        private void CanUndoCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var viewmodel = DataContext as MarkingSpacesControlViewModel;
            if (viewmodel != null) e.CanExecute = viewmodel.UndoRedoManager.CanUndo();
            e.Handled = true;
        }

        private void RedoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var viewmodel = DataContext as MarkingSpacesControlViewModel;
            if (viewmodel != null) viewmodel.UndoRedoManager.RedoCommand.Execute();

            e.Handled = true;
        }

        private void UndoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var viewmodel = DataContext as MarkingSpacesControlViewModel;
            if (viewmodel != null) viewmodel.UndoRedoManager.UndoCommand.Execute();
            e.Handled = true;
        }
    }
}
