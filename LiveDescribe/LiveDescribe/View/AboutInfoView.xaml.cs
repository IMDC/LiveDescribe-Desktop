using LiveDescribe.ViewModel;
using System.Windows;

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
