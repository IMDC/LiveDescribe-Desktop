using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class DescriptionCanvasViewModel : ViewModelBase
    {
        private readonly DescriptionViewModel _descriptionViewModel;

        #region Events
        public EventHandler<MouseEventArgs> DescriptionCanvasMouseUpEvent;
        public EventHandler<MouseEventArgs> DescriptionCanvasMouseMoveEvent;
        #endregion

        public DescriptionCanvasViewModel(DescriptionViewModel descriptionViewModel)
        {
            _descriptionViewModel = descriptionViewModel;
            DescriptionCanvasMouseUpCommand = new RelayCommand<MouseEventArgs>(DescriptionCanvasMouseUp, param => true);
            DescriptionCanvasMouseMoveCommand = new RelayCommand<MouseEventArgs>(DescriptionCanvasMouseMove, param => true);
        }

        #region Commands
        public RelayCommand<MouseEventArgs> DescriptionCanvasMouseUpCommand { private set; get; }
        public RelayCommand<MouseEventArgs> DescriptionCanvasMouseMoveCommand { private set; get; }
        #endregion

        #region Binding Properties
        public ObservableCollection<Description> AllDescriptions
        {
            get { return _descriptionViewModel.AllDescriptions; }
        }
        #endregion

        #region Binding Functions
        public void DescriptionCanvasMouseUp(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = DescriptionCanvasMouseUpEvent;
            if (handler != null) handler(this, e);
        }

        public void DescriptionCanvasMouseMove(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = DescriptionCanvasMouseMoveEvent;
            if (handler != null) handler(this, e);
        }
        #endregion
    }
}