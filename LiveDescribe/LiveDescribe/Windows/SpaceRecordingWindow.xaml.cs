﻿using LiveDescribe.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace LiveDescribe.Windows
{
    /// <summary>
    /// Interaction logic for SpaceRecordingWindow.xaml
    /// </summary>
    public partial class SpaceRecordingWindow : Window
    {
        private readonly SpaceRecordingViewModel _viewModel;
        private List<PositionalStringToken>.Enumerator _enumerator;

        public SpaceRecordingWindow(SpaceRecordingViewModel vm)
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
                _enumerator = _viewModel.SpaceTextTokenizer.Tokens.GetEnumerator();
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
                    SpaceTextBox.Select(token.StartIndex, token.Length);
                }
                else
                {
                    SpaceTextBox.Select(SpaceTextBox.Text.Length - 1, 0);
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
            if (_viewModel.Recorder.IsRecording)
                e.Handled = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SpaceRecordingView_OnClosing(object sender, CancelEventArgs e)
        {
            _viewModel.StopEverything();
        }
    }
}
