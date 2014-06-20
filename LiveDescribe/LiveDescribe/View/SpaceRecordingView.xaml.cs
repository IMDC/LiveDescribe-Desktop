using LiveDescribe.ViewModel;
using System.Windows;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for SpaceRecordingView.xaml
    /// </summary>
    public partial class SpaceRecordingView : Window
    {
        public SpaceRecordingView(SpaceRecordingViewModel viewModel)
        {
            InitializeComponent();

            viewModel.CloseRequested += (sender, args) =>
            {
                DialogResult = true;
                Close();
            };

            DataContext = viewModel;
        }
    }
}
