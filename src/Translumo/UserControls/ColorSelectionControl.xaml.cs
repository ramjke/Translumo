using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Translumo.Controls
{
    /// <summary>
    /// Interaction logic for ColorSelectionControl.xaml
    /// </summary>
    public partial class ColorSelectionControl : UserControl
    {
        public static DependencyProperty ColorPickedCommandProperty
            = DependencyProperty.Register("ColorPickedCommand", typeof(ICommand), typeof(ColorSelectionControl));

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                "SelectedColor", typeof(Color), typeof(ColorSelectionControl),
                new FrameworkPropertyMetadata(
                    defaultValue: default(Color), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedColorChangedCallback));

        private static void SelectedColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((Color)e.NewValue == (Color)e.OldValue)
            {
                return;
            }

            var targetControl = (ColorSelectionControl)d;
            targetControl.cpWindowColor.Color = (Color)e.NewValue;
        }

        public ICommand ColorPickedCommand
        {
            get { return (ICommand)GetValue(ColorPickedCommandProperty); }
            set { SetValue(ColorPickedCommandProperty, value); }
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public string Caption
        {
            get => (string)lCaption.Content;
            set => lCaption.Content = value;
        }

        public ColorSelectionControl()
        {
            InitializeComponent();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            SetValue(SelectedColorProperty, cpWindowColor.Color);
            ColorPickedCommand.Execute(true);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ColorPickedCommand.Execute(false);
        }
    }
}
