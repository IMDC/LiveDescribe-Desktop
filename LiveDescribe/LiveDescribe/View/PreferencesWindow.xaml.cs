using LiveDescribe.ViewModel;
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
            datacontext.RetrieveApplicationSettings();
            datacontext.RequestClose += (sender, args) => Close();
        }
    }
}
