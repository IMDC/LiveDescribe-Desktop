using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;

namespace LiveDescribe.ViewModel
{
    public class SpaceRecordingViewModel : ViewModelBase
    {
        #region Fields
        private Description _description;
        private Space _space;
        #endregion

        #region Constructor
        public SpaceRecordingViewModel(Space space)
        {
            InitCommands();

            _description = null;
            Space = space;
        }

        public void InitCommands()
        {
            RecordDescription = new RelayCommand(
                canExecute: () => _space != null,
                execute: () =>
                {
                    
                });

            PlayRecordedDescription = new RelayCommand(
                canExecute: () => _description != null,
                execute: () =>
                {

                });

            SaveDescription = new RelayCommand(
                canExecute: () => _description != null,
                execute: () =>
                {

                });
        }
        #endregion

        #region Commands
        public ICommand RecordDescription { private set; get; }
        public ICommand PlayRecordedDescription { private set; get; }
        public ICommand SaveDescription { private set; get; }
        #endregion

        #region Properties
        public string Text
        {
            set
            {
                _space.SpaceText = value;
                RaisePropertyChanged();
            }
            get { return _space.SpaceText; }
        }

        public string TimeStamp { set; get; }

        public Description Description
        {
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
            get { return _description; }
        }

        public Space Space
        {
            set
            {
                _space = value;
                RaisePropertyChanged();
            }
            get { return _space; }
        }
        #endregion
    }
}
