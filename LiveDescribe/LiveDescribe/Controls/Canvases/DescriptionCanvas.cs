using LiveDescribe.Extensions;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.Resources.UiStrings;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LiveDescribe.Controls.Canvases
{
    public class DescriptionCanvas : GeometryImageCanvas
    {
        #region Fields
        private DescriptionCanvasViewModel _canvasViewModel;
        private Brush _regularDescriptionBrush;
        private Brush _extendedDescriptionBrush;
        private Image _descriptionImage;

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
                if (_canvasViewModel == null || Math.Abs(args.PreviousSize.Height - args.NewSize.Height) < 0.01)
                    return;

                foreach (var description in _canvasViewModel.AllDescriptions)
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
            if (_canvasViewModel == null || Width == 0 || VisibleWidth == 0
                || _canvasViewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            Children.Remove(_descriptionImage);

            var drawingVisual = new DrawingVisual();

            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                double beginTimeMsec = XPosToMilliseconds(VisibleX);
                double endTimeMsec = XPosToMilliseconds(VisibleX + VisibleWidth);

                foreach (var description in _canvasViewModel.AllDescriptions)
                {
                    if (IsIntervalVisible(description, beginTimeMsec, endTimeMsec))
                    {
                        var rect = new Rect(description.X, description.Y, description.Width, description.Height);

                        Brush rectBrush;

                        if (description.IsSelected)
                            rectBrush = SelectedItemBrush;
                        else if (description.IsExtendedDescription)
                            rectBrush = _extendedDescriptionBrush;
                        else
                            rectBrush = _regularDescriptionBrush;

                        if (description.WaveformImage == null)
                            description.WaveformImage = RenderTargetBitmapFactory.CreateDescriptionWaveForm(description,
                                rect, Width);

                        dc.DrawImage(description.WaveformImage, rect);

                        dc.DrawRectangle(rectBrush, LinePen, rect);
                    }
                }
            }

            AddImageToCanvas(ref _descriptionImage, drawingVisual);
        }

        public override void DrawMouseSelection()
        {
            //TODO: Replace this logic.
            Draw();
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

            if (_canvasViewModel == null)
                return;

            var clickPoint = e.GetPosition(this);
            bool descriptionFound = false;

            foreach (var description in _canvasViewModel.AllDescriptions)
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

            if (_canvasViewModel == null)
                return;

            if (MouseSelection.Action != IntervalMouseAction.None)
            {
                if (MouseSelection.HasItemBeenModified())
                    MouseSelection.AddChangesTo(_canvasViewModel.UndoRedoManager);

                MouseSelection.CompleteModificationAction();
                Mouse.Capture(null);
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);

            if (_canvasViewModel == null)
                return;

            ContextMenu.Items.Clear();

            bool descriptionFound = false;

            var point = Mouse.GetPosition(this);

            foreach (var description in _canvasViewModel.AllDescriptions)
            {
                if (IsBetweenBounds(description.X - SelectionPixelWidth, point.X,
                    description.X + description.Width + SelectionPixelWidth))
                {
                    ContextMenu.Items.Add(new MenuItem
                    {
                        Header = UiStrings.Command_GoToDescription,
                        Command = description.NavigateToCommand,
                    });
                    ContextMenu.Items.Add(new MenuItem
                    {
                        Header = UiStrings.Command_DeleteDescription,
                        Command = description.DeleteCommand,
                    });

                    descriptionFound = true;
                }
            }

            if (!descriptionFound)
                e.Handled = true;
        }

        #endregion

        #region Event Handlers
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _canvasViewModel = e.NewValue as DescriptionCanvasViewModel;

            if (_canvasViewModel != null)
            {
                _canvasViewModel.AllDescriptions.CollectionChanged += CollectionChanged_TrackPropertyListeners;
            }

            var oldViewModel = e.OldValue as DescriptionCanvasViewModel;

            if (oldViewModel != null)
            {
                oldViewModel.AllDescriptions.CollectionChanged -= CollectionChanged_TrackPropertyListeners;
            }
        }

        protected override void CollectionChanged_TrackPropertyListeners(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.CollectionChanged_TrackPropertyListeners(sender, e);

            Draw();
        }
        #endregion
    }
}
