using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace Translumo
{
    public partial class SelectionAreaWindow : Window
    {
        public Point MouseInitialPos { get; private set; }
        public Point MouseEndPos { get; private set; }
        public RectangleF SelectedArea { get; private set; }

        private bool _mouseIsDown = false; // Set to 'true' when mouse is held down.
        private Point _relativeInitialPos; // The point where the mouse button was clicked down.

        private readonly bool _readonlyMode = false;

        public SelectionAreaWindow()
        {
            InitializeComponent();
        }

        public SelectionAreaWindow(RectangleF rectangle)
        {
            InitializeComponent();

            this._readonlyMode = true;
            this.SelectedArea = rectangle;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || _readonlyMode)
            {
                CloseDialog(true);
                return;
            }

            // Capture and track the mouse.
            _mouseIsDown = true;
            _relativeInitialPos = e.GetPosition(this);
            MouseInitialPos = this.PointToScreen(_relativeInitialPos);

            theGrid.CaptureMouse();

            DrawSelection(_relativeInitialPos.X, _relativeInitialPos.Y, 0, 0);
        }

        private void DrawSelection(double x, double y, double width, double height)
        {
            // Initial placement of the drag selection box.         
            Canvas.SetLeft(selectionBox, x);
            Canvas.SetTop(selectionBox, y);
            selectionBox.Width = width;
            selectionBox.Height = height;

            selectionBox.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_readonlyMode)
            {
                return;
            }

            // Release the mouse capture and stop tracking it.
            _mouseIsDown = false;
            theGrid.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            MouseEndPos = this.PointToScreen(e.GetPosition(this));
            SelectedArea = CalculateArea(MouseInitialPos, MouseEndPos);

            CloseDialog(false);
        }

        private RectangleF CalculateArea(Point firstPoint, Point secondPoint)
        {
            return new RectangleF((int)Math.Min(firstPoint.X, secondPoint.X),
                (int)Math.Min(firstPoint.Y, secondPoint.Y),
                (int)Math.Abs(firstPoint.X - secondPoint.X),
                (int)Math.Abs(firstPoint.Y - secondPoint.Y));
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseIsDown)
            {
                // When the mouse is held down, reposition the drag selection box.

                Point mousePos = e.GetPosition(theGrid);

                if (_relativeInitialPos.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, _relativeInitialPos.X);
                    selectionBox.Width = mousePos.X - _relativeInitialPos.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = _relativeInitialPos.X - mousePos.X;
                }

                if (_relativeInitialPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, _relativeInitialPos.Y);
                    selectionBox.Height = mousePos.Y - _relativeInitialPos.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = _relativeInitialPos.Y - mousePos.Y;
                }
            }
        }

        private void CloseDialog(bool cancellation)
        {
            this.DialogResult = !cancellation;
            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void SelectionAreaWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!SelectedArea.IsEmpty && _readonlyMode)
            {
                var leftUpperPoint = this.PointFromScreen(new Point(SelectedArea.X, SelectedArea.Y));
                var rightBottomPoint = this.PointFromScreen(new Point(SelectedArea.Right, SelectedArea.Bottom));
                DrawSelection(leftUpperPoint.X, leftUpperPoint.Y, rightBottomPoint.X - leftUpperPoint.X, rightBottomPoint.Y - leftUpperPoint.Y);
            }
        }

        private void SelectionAreaWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_readonlyMode)
            {
                CloseDialog(true);
            }
        }
    }
}
