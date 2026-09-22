using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Translumo.Controls
{
    [TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox))]
    [TemplatePart(Name = IncreasePartName, Type = typeof(ButtonBase))]
    [TemplatePart(Name = DecreasePartName, Type = typeof(ButtonBase))]
    public class NumericInput : Control
    {
        private const string TextBoxPartName = "PART_TextBox";
        private const string IncreasePartName = "PART_Increase";
        private const string DecreasePartName = "PART_Decrease";

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int), typeof(NumericInput),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(int), typeof(NumericInput), new PropertyMetadata(0, OnRangeChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(int), typeof(NumericInput), new PropertyMetadata(int.MaxValue, OnRangeChanged));

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step), typeof(int), typeof(NumericInput), new PropertyMetadata(1));

        private TextBox _textBox;
        private ButtonBase _increaseButton;
        private ButtonBase _decreaseButton;

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetCurrentValue(ValueProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetCurrentValue(MinimumProperty, value);
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetCurrentValue(MaximumProperty, value);
        }

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetCurrentValue(StepProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_increaseButton != null)
            {
                _increaseButton.Click -= OnIncreaseClick;
            }

            if (_decreaseButton != null)
            {
                _decreaseButton.Click -= OnDecreaseClick;
            }

            if (_textBox != null)
            {
                _textBox.LostFocus -= OnTextBoxLostFocus;
                _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            }

            _textBox = GetTemplateChild(TextBoxPartName) as TextBox;
            _increaseButton = GetTemplateChild(IncreasePartName) as ButtonBase;
            _decreaseButton = GetTemplateChild(DecreasePartName) as ButtonBase;

            if (_increaseButton != null)
            {
                _increaseButton.Click += OnIncreaseClick;
            }

            if (_decreaseButton != null)
            {
                _decreaseButton.Click += OnDecreaseClick;
            }

            if (_textBox != null)
            {
                _textBox.LostFocus += OnTextBoxLostFocus;
                _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            }

            UpdateText();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!IsKeyboardFocusWithin)
            {
                return;
            }

            Shift(e.Delta > 0 ? Step : -Step);
            e.Handled = true;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var input = (NumericInput)d;
            var value = (int)baseValue;

            return Math.Max(input.Minimum, Math.Min(input.Maximum, value));
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericInput)d).UpdateText();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ValueProperty);
        }

        private void OnIncreaseClick(object sender, RoutedEventArgs e)
        {
            Shift(Step);
        }

        private void OnDecreaseClick(object sender, RoutedEventArgs e)
        {
            Shift(-Step);
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Shift(Step);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                Shift(-Step);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                CommitText();
                e.Handled = true;
            }
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            CommitText();
        }

        private void Shift(int delta)
        {
            Value = Math.Max(Minimum, Math.Min(Maximum, Value + delta));
        }

        private void CommitText()
        {
            if (_textBox == null)
            {
                return;
            }

            if (int.TryParse(_textBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsed))
            {
                Value = Math.Max(Minimum, Math.Min(Maximum, parsed));
            }

            UpdateText();
        }

        private void UpdateText()
        {
            if (_textBox == null)
            {
                return;
            }

            var text = Value.ToString(CultureInfo.CurrentCulture);
            if (_textBox.Text != text)
            {
                _textBox.Text = text;
            }
        }
    }
}
