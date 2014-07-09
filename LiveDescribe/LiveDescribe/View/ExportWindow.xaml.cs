using System;
using System.Collections.Generic;
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
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly ExportWindowViewModel _viewModel;

        public ExportWindow(ExportWindowViewModel dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
            _viewModel = dataContext;

            dataContext.ProjectExported += (sender, args) =>
            {
                DialogResult = true;
                Close();
            };

            //Set state for CompressAudio
            CompressAudio_OnChecked(this, null);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CompressAudio_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                _viewModel.CompressAudio= YesCompressButton.IsChecked == true;
        }
    }
}