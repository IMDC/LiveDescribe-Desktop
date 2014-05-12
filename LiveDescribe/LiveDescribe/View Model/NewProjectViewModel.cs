using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using LiveDescribe.View_Model;

namespace LiveDescribe.View
{
    public class NewProjectViewModel : ViewModelBase
    {
        /// <summary>
        /// Creates a NewProjectView and attaches an instance of NewProjectViewModel to it. Returns
        /// only when the window is closed.
        /// </summary>
        /// <returns>The return value of window.ShowDialog.</returns>
        public static bool? CreateWindow()
        {
            var viewModel = new NewProjectViewModel();
            var view = new NewProjectView
            {
                DataContext = viewModel,
            };

            return view.ShowDialog();
        }
    }
}
