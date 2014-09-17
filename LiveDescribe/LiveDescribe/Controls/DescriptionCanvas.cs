using LiveDescribe.Extensions;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LiveDescribe.Controls
{
    public class DescriptionCanvas : GeometryImageCanvas
    {
        #region Fields
        private DescriptionCanvasViewModel _viewModel;
        private Brush _regularDescriptionBrush;
        private Brush _extendedDescriptionBrush;
        private Image _regularDescriptionImage;
        private Image _extendedDescriptionImage;

        #endregion

        #region Constructor

        public DescriptionCanvas()
        {
            Background = Brushes.Transparent;

            if (!DesignerProperties.GetIsInDesignMode(this))
                SetBrushes();

            MouseSelection = CanvasMouseSelection.NoSelection;

            ContextMenu = new ContextMenu();

            InitEventHandlers();
        }

        private void InitEventHandlers()
        {
            DataContextChanged += OnDataContextChanged;

            //Resize all spaces to fit the height of this canvas.
            SizeChanged += (sender, args) =>
            {
                if (_viewModel == null || Math.Abs(args.PreviousSize.Height - args.NewSize.Height) < 0.01)
                    return;

                foreach (var description in _viewModel.AllDescriptions)
                    description.Height = ActualHeight;
            };

            Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ColourScheme")
                    SetBrushes();
            };
        }
        #endregion

        #region Canvas Drawing
        public override void Draw()
        {
            if (_viewModel == null || Width == 0 || VisibleWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            Children.Remove(_regularDescriptionImage);
            Children.Remove(_extendedDescriptionImage);
            Children.Remove(SelectedImage);

            var regularDescriptionGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
            var extendedDescriptionGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
            var selectedItemGroup = new GeometryGroup();

            double beginTimeMsec = XPosToMilliseconds(VisibleX);
            double endTimeMsec = XPosToMilliseconds(VisibleX + VisibleWidth);

            foreach (var description in _viewModel.AllDescriptions)
            {
                if (IsIntervalVisible(description, beginTimeMsec, endTimeMsec))
                {
                    var rect = new RectangleGeometry(new Rect(description.X, description.Y, description.Width,
                            description.Height));

                    if (description.IsSelected)
                        selectedItemGroup.Children.Add(rect);
                    else if (description.IsExtendedDescription)
                        extendedDescriptionGroup.Children.Add(rect);
                    else
                        regularDescriptionGroup.Children.Add(rect);
                }
            }

            AddImageToCanvas(ref _regularDescriptionImage, regularDescriptionGroup, _regularDescriptionBrush);
            AddImageToCanvas(ref _extendedDescriptionImage, extendedDescriptionGroup, _extendedDescriptionBrush);
            AddImageToCanvas(ref SelectedImage, selectedItemGroup, SelectedItemBrush);
        }

        protected override sealed void SetBrushes()
        {
            _regularDescriptionBrush = new SolidColorBrush(Settings.Default.ColourScheme.RegularDescriptionColour);
            _regularDescriptionBrush.Freeze();

            _extendedDescriptionBrush = new SolidColorBrush(Settings.Default.ColourScheme.ExtendedDescriptionColour);
            _extendedDescriptionBrush.Freeze();

            SelectedItemBrush = new SolidColorBrush(Settings.Default.ColourScheme.SelectedItemColour);
            SelectedItemBrush.Freeze();

            Draw();
        }
        #endregion

        #region Mouse Interaction

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (_viewModel == null)
                return;

            var clickPoint = e.GetPosition(this);
            bool descriptionFound = false;

            foreach (var description in _viewModel.AllDescriptions)
            {
                if (IsBetweenBounds(description.X, clickPoint.X, description.X + description.Width))
                {
                    SelectDescription(description, clickPoint);
                    descriptionFound = true;
                }
                else
                    description.IsSelected = false;
            }

            if (!descriptionFound)
                MouseSelection = CanvasMouseSelection.NoSelection;

            Draw();
        }

        public void SelectDescription(Description description, Point clickPoint)
        {
            description.IsSelected = true;
            description.MouseDownCommand.Execute();

            MouseSelection = new CanvasMouseSelection(IntervalMouseAction.Dragging, description,
                XPosToMilliseconds(clickPoint.X) - description.StartInVideo);

            CaptureMouse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MouseSelection.Action != IntervalMouseAction.Dragging || MouseSelection.Item.LockedInPlace)
                return;

            var mousePos = e.GetPosition(this);

            double startTime = BoundBetween(0, XPosToMilliseconds(mousePos.X) - MouseSelection.MouseClickTimeDifference,
                VideoDurationMsec - MouseSelection.Item.Duration);
            MouseSelection.Item.MoveInterval(startTime);
            DrawMouseSelection();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_viewModel == null)
                return;

            if (MouseSelection.Action != IntervalMouseAction.None)
            {
                if (MouseSelection.HasItemBeenModified())
                    MouseSelection.AddChangesTo(_viewModel.UndoRedoManager);

                MouseSelection.CompleteModificationAction();
                Mouse.Capture(null);
            }
        }

        #endregion

        #region Event Handlers
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as DescriptionCanvasViewModel;

            if (_viewModel != null)
            {
                _viewModel.AllDescriptions.CollectionChanged += CollectionChanged_TrackPropertyListeners;
            }

            var oldViewModel = e.OldValue as DescriptionCanvasViewModel;

            if (oldViewModel != null)
            {
                _viewModel.AllDescriptions.CollectionChanged -= CollectionChanged_TrackPropertyListeners;
            }
        }

        #endregion
    }
}
