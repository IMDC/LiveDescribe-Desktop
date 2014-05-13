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
        public NewProjectView(NewProjectViewModel dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;

            dataContext.ProjectCreated += dataContext_ProjectCreated;
        }

        void dataContext_ProjectCreated(object sender, EventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
