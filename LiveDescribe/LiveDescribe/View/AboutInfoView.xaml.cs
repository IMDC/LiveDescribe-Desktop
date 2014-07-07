using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveDescribe.ViewModel;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for AboutInfoView.xaml
    /// </summary>
    public partial class AboutInfoView : Window
    {
        public AboutInfoView()
        {
            InitializeComponent();

            DataContext = new AboutInfoViewModel();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
