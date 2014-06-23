using System;
using System.Collections.Generic;
using System.Windows.Documents;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System.Windows;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for SpaceRecordingView.xaml
    /// </summary>
    public partial class SpaceRecordingView : Window
    {
        private readonly SpaceRecordingViewModel _viewModel;
        private List<PositionalStringToken>.Enumerator _enumerator;

        public SpaceRecordingView(SpaceRecordingViewModel vm)
        {
            InitializeComponent();

            _viewModel = vm;

            _viewModel.CloseRequested += (sender, args) =>
            {
                DialogResult = true;
                Close();
            };

            _viewModel.RecordingStarted += (sender, args) =>
            {
                SpaceTextBox.Focus();
                SpaceTextBox.IsReadOnly = true;
                var tokenizer = new PositionalStringTokenizer(_viewModel.Space.SpaceText);
                tokenizer.Tokenize();
                _enumerator = tokenizer.Tokens.GetEnumerator();
            };

            _viewModel.RecordingEnded += (sender, args) =>
            {
                SpaceTextBox.Select(0, 0);
                SpaceTextBox.IsReadOnly = false;
            };

            _viewModel.NextWordSelected += (sender, args) =>
            {
                if (_enumerator.MoveNext())
                {
                    var token = _enumerator.Current;
                    SpaceTextBox.Select(token.StartIndex,token.Length);
                }
            };

            DataContext = _viewModel;
        }

        private void SpaceTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            /* This is a hack to ensure that the textbox's selection will be visible when recording
             * a description. When focus is lost, the highlighted text will be coloured grey,
             * making it difficult to see.
             */
            e.Handled = true;
        }
    }
}
