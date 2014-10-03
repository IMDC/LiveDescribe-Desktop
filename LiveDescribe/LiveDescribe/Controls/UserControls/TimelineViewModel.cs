﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Controls.Canvases;
using System.Windows.Input;

namespace LiveDescribe.Controls.UserControls
{
    public class TimelineViewModel : ViewModelBase
    {
        private readonly AudioCanvasViewModel _audioCanvasViewModel;
        private readonly DescriptionCanvasViewModel _descriptionCanvasViewModel;
        private readonly MediaViewModel _mediaViewModel;
        private readonly NumberLineViewModel _numberLineViewModel;

        #region Constructor
        public TimelineViewModel(AudioCanvasViewModel audioCanvasViewModel,
            DescriptionCanvasViewModel descriptionCanvasViewModel,
            MediaViewModel mediaViewModel,
            NumberLineViewModel numberLineViewModel)
        {
            _audioCanvasViewModel = audioCanvasViewModel;
            _descriptionCanvasViewModel = descriptionCanvasViewModel;
            _mediaViewModel = mediaViewModel;
            _numberLineViewModel = numberLineViewModel;

            InitCommands();
        }

        public void InitCommands()
        {
            ZoomIn = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    //TODO this
                });

            ZoomOut = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    //TODO this
                });
        }
        #endregion

        #region Commands
        public ICommand ZoomIn { get; private set; }
        public ICommand ZoomOut { get; private set; }
        #endregion

        #region Properties
        public NumberLineViewModel NumberLineViewModel
        {
            get { return _numberLineViewModel; }
        }

        public MediaViewModel MediaViewModel
        {
            get { return _mediaViewModel; }
        }

        public DescriptionCanvasViewModel DescriptionCanvasViewModel
        {
            get { return _descriptionCanvasViewModel; }
        }

        public AudioCanvasViewModel AudioCanvasViewModel
        {
            get { return _audioCanvasViewModel; }
        }
        #endregion
    }
}
