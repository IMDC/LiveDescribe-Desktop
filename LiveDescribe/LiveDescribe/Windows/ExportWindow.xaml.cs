using System.Windows;

namespace LiveDescribe.Windows
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly ExportViewModel _viewModel;

        public ExportWindow(ExportViewModel dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
            _viewModel = dataContext;

            dataContext.ProjectExported += (sender, args) =>
            {
                DialogResult = true;
                Close();
            };

            //Set state for CompressAudio
            CompressAudio_OnChecked(this, null);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CompressAudio_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                _viewModel.CompressAudio = YesCompressButton.IsChecked == true;
        }
    }
}