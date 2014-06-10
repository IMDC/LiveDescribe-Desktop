using LiveDescribe.View_Model;
using System.Windows;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow(PreferencesViewModel datacontext)
        {
            InitializeComponent();

            DataContext = datacontext;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
