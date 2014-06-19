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
    /// Interaction logic for SpaceRecordingView.xaml
    /// </summary>
    public partial class SpaceRecordingView : Window
    {
        public SpaceRecordingView(SpaceRecordingViewModel viewModel)
        {
            InitializeComponent();

            viewModel.CloseRequested += (sender, args) =>
            {
                DialogResult = true;
                Close();
            };

            DataContext = viewModel;
        }
    }
}
