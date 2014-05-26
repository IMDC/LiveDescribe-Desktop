using LiveDescribe.View_Model;
using System;
using System.Windows;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for NewProjectView.xaml
    /// </summary>
    public partial class NewProjectView : Window
    {
        private readonly NewProjectViewModel _viewModel;

        public NewProjectView(NewProjectViewModel dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
            _viewModel = dataContext;

            dataContext.ProjectCreated += (sender, args) =>
            {
                DialogResult = true;
                this.Close();
            };

            //Set state for CopyVideo
            CopyVideo_OnChecked(this, null);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void CopyVideo_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                _viewModel.CopyVideo = YesCopyButton.IsChecked == true;
        }
    }
}
