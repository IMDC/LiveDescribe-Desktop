using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LiveDescribe.Controls.UserControls
{
    /// <summary>
    /// Interaction logic for CountdownControl.xaml
    /// </summary>
    public partial class CountdownControl : UserControl
    {
        public CountdownControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// Adds a property changed listener to the new datacontext and removes it from the old one.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Args</param>
        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = e.NewValue as CountdownViewModel;
            if (viewModel != null)
                viewModel.PropertyChanged += ViewModelOnPropertyChanged;

            var oldViewModel = e.OldValue as CountdownViewModel;
            if (oldViewModel != null)
                oldViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var viewModel = (CountdownViewModel)sender;

            if (args.PropertyName == "Visible")
            {
                if (viewModel.Visible)
                    Panel.SetZIndex(this, 2);
                else
                    Panel.SetZIndex(this, -1);
            }
        }
    }
}
