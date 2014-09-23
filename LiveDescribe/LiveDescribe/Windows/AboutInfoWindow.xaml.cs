using System.Windows;

namespace LiveDescribe.Windows
{
    /// <summary>
    /// Interaction logic for AboutInfoWindow.xaml
    /// </summary>
    public partial class AboutInfoWindow : Window
    {
        public AboutInfoWindow()
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
