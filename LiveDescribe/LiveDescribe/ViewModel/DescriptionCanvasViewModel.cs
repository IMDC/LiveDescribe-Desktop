using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class DescriptionCanvasViewModel : ViewModelBase
    {
        private readonly DescriptionCollectionViewModel _descriptionCollectionViewModel;
        private LiveDescribeVideoStates _currentVideoState;
        #region Events
        public EventHandler<MouseEventArgs> DescriptionCanvasMouseUpEvent;
        public EventHandler<MouseEventArgs> DescriptionCanvasMouseMoveEvent;
        #endregion

        public DescriptionCanvasViewModel(DescriptionCollectionViewModel descriptionCollectionViewModel, ILiveDescribePlayer videoMedia)
        {
            _descriptionCollectionViewModel = descriptionCollectionViewModel;
            DescriptionCanvasMouseUpCommand = new RelayCommand<MouseEventArgs>(DescriptionCanvasMouseUp, param => true);
            DescriptionCanvasMouseMoveCommand = new RelayCommand<MouseEventArgs>(DescriptionCanvasMouseMove, param => true);
            videoMedia.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("CurrentState"))
                    CurrentVideoState = videoMedia.CurrentState;
            };
        }

        #region Commands
        public RelayCommand<MouseEventArgs> DescriptionCanvasMouseUpCommand { private set; get; }
        public RelayCommand<MouseEventArgs> DescriptionCanvasMouseMoveCommand { private set; get; }
        #endregion

        #region Binding Properties
        public ObservableCollection<Description> AllDescriptions
        {
            get { return _descriptionCollectionViewModel.AllDescriptions; }
        }

        public LiveDescribeVideoStates CurrentVideoState
        {
            set
            {
                _currentVideoState = value;
                RaisePropertyChanged();
            }
            get { return _currentVideoState; }
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