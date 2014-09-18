using System.Windows;
using System.Windows.Controls;
using LiveDescribe.ViewModel.Controls;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for AudioSourceSettingsControl.xaml
    /// </summary>
    public partial class AudioSourceSettingsControl : UserControl
    {
        public AudioSourceSettingsControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = e.NewValue as AudioSourceSettingsControlViewModel;
            if (viewModel != null)
                viewModel.IsVisible = IsVisible;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = DataContext as AudioSourceSettingsControlViewModel;
            if (viewModel != null)
                viewModel.IsVisible = (bool)e.NewValue;
        }
    }
}
