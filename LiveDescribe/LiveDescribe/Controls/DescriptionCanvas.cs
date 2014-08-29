using LiveDescribe.Extensions;
using LiveDescribe.Properties;
using LiveDescribe.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiveDescribe.Controls
{
    public class DescriptionCanvas : GeometryImageCanvas
    {
        #region Fields
        private DescriptionCanvasViewModel _viewModel;
        private readonly Pen _linePen;
        private CanvasMouseSelection _mouseSelection;
        private Brush _regularDescriptionBrush;
        private Brush _extendedDescriptionBrush;
        private Brush _selectedItemBrush;
        private Image _regularDescriptionImage;
        private Image _extendedDescriptionImage;
        private Image _selectedImage;

        #endregion

        #region Constructor

        public DescriptionCanvas()
        {
            _linePen = new Pen(Brushes.Black, 1);
            _linePen.Freeze();

            if (!DesignerProperties.GetIsInDesignMode(this))
                SetBrushes();

            _mouseSelection = CanvasMouseSelection.NoSelection;

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

        public override void Draw()
        {
            if (_viewModel == null || Width == 0 || VisibleWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            if (Children.Contains(_regularDescriptionImage))
                Children.Remove(_regularDescriptionImage);
            if (Children.Contains(_extendedDescriptionImage))
                Children.Remove(_extendedDescriptionImage);
            if (Children.Contains(_selectedImage))
                Children.Remove(_selectedImage);


            var regularDescriptionGroup = new GeometryGroup();
            var extendedDescriptionGroup = new GeometryGroup();
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

            if (0 < regularDescriptionGroup.Children.Count)
            {
                _regularDescriptionImage = regularDescriptionGroup.CreateImage(_regularDescriptionBrush, _linePen);

                Children.Add(_regularDescriptionImage);

                //The Image has to be set to the smallest X value of the visible regularDescriptions.
                double minX = regularDescriptionGroup.Children[0].Bounds.X;
                for (int i = 1; i < regularDescriptionGroup.Children.Count; i++)
                {
                    minX = Math.Min(minX, regularDescriptionGroup.Children[i].Bounds.X);
                }

                SetLeft(_regularDescriptionImage, minX);
                SetTop(_regularDescriptionImage, 0);
            }
        }

        protected override sealed void SetBrushes()
        {
            _regularDescriptionBrush = new SolidColorBrush(Settings.Default.ColourScheme.RegularDescriptionColour);
            _regularDescriptionBrush.Freeze();

            _extendedDescriptionBrush = new SolidColorBrush(Settings.Default.ColourScheme.ExtendedDescriptionColour);
            _extendedDescriptionBrush.Freeze();

            _selectedItemBrush = new SolidColorBrush(Settings.Default.ColourScheme.SelectedItemColour);
            _selectedItemBrush.Freeze();

            Draw();
        }

        #region Event Handlers
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as DescriptionCanvasViewModel;

            if (_viewModel != null)
            {
            }

            var oldViewModel = e.OldValue as DescriptionCanvasViewModel;

            if (oldViewModel != null)
            {
            }
        }
        #endregion
    }
}
