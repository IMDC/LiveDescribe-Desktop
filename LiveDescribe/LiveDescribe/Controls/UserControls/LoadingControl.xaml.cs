using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LiveDescribe.Controls.UserControls
{
    /// <summary>
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : UserControl
    {
        public LoadingControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = e.NewValue as LoadingViewModel;
            if (viewModel != null)
                viewModel.PropertyChanged += ViewModelOnPropertyChanged;

            var oldViewModel = e.OldValue as LoadingViewModel;
            if (oldViewModel != null)
                oldViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var viewModel = (LoadingViewModel)sender;

            /* Set LoadingBorder to appear in front of everything when visible, otherwise put
            * it behind everything. This allows it to sit behind in the XAML viewer.
            */
            if (e.PropertyName.Equals("Visible"))
            {
                if (viewModel.Visible)
                    Panel.SetZIndex(this, 2);
                else
                    Panel.SetZIndex(this, -1);
            }
        }
    }
}
