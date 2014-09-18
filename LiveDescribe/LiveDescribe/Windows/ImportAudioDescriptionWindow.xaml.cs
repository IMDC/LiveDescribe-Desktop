using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LiveDescribe.Windows
{
    /// <summary>
    /// Interaction logic for ImportAudioDescription.xaml
    /// </summary>
    public partial class ImportAudioDescriptionWindow : Window
    {
        public ImportAudioDescriptionWindow(ImportAudioDescriptionViewModel dataContext)
        {
            DataContext = dataContext;

            dataContext.OnImportDescription += (sender, args) =>
            {
                DialogResult = true;
                Close();
            };
            InitializeComponent();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void StartInVideoTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var textBox = (TextBox)sender;
                BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                if (be != null)
                    be.UpdateSource();
            }
        }
    }
}
