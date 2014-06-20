using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LiveDescribe.Controls
{
    /// <summary>
    /// Interaction logic for MarkingSpacesControl.xaml
    /// </summary>
    public partial class MarkingSpacesControl : UserControl
    {
        public MarkingSpacesControl()
        {
            InitializeComponent();
        }

        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var textBox = (TextBox) sender;
                BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                if (be != null)
                    be.UpdateSource();
            }
        }
    }
}
