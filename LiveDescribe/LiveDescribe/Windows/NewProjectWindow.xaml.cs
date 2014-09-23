using System.Windows;

namespace LiveDescribe.Windows
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class NewProjectWindow : Window
    {
        private readonly NewProjectViewModel _viewModel;

        public NewProjectWindow(NewProjectViewModel dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
            _viewModel = dataContext;

            dataContext.ProjectCreated += (sender, args) =>
            {
                DialogResult = true;
                Close();
            };

            //Set state for CopyVideo
            CopyVideo_OnChecked(this, null);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CopyVideo_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                _viewModel.CopyVideo = YesCopyButton.IsChecked == true;
        }
    }
}
