using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveDescribe.View_Model;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

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
