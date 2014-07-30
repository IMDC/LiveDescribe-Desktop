using LiveDescribe.ViewModel;
using System;
using System.Windows;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private readonly PreferencesViewModel _preferencesViewModel;

        public PreferencesWindow(PreferencesViewModel datacontext)
        {
            InitializeComponent();

            DataContext = datacontext;
            _preferencesViewModel = datacontext;
            _preferencesViewModel.RetrieveApplicationSettings();
            _preferencesViewModel.RequestClose += (sender, args) => Close();
        }

        private void PreferencesWindow_OnClosed(object sender, EventArgs e)
        {
            //Call the method from here so it gets called even when you click the X
            _preferencesViewModel.CloseCleanup();
        }
    }
}
