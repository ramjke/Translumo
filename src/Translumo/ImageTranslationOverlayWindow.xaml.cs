using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Translumo.Infrastructure.Language;
using Translumo.Processing.ImageTranslation;
using Point = System.Windows.Point;
using RectangleF = System.Drawing.RectangleF;

namespace Translumo
{
    /// <summary>
    /// Google Lens style overlay: shows a frozen snapshot of the selected region and paints the
    /// translation of each recognized line over its original position. A floating toolbar lets the
    /// user override the source recognizer language and pick the target language (both re-run the
    /// pipeline via the <c>retranslate</c> delegate) and copy all translations.
    /// </summary>
    public partial class ImageTranslationOverlayWindow : Window
    {
        public sealed class TargetLanguageOption
        {
            public Languages Value { get; init; }
            public string Display { get; init; }
            public override string ToString() => Display;
        }

        public sealed class SourceLanguageOption
        {
            /// <summary>BCP-47 recognizer tag, or null for auto-detect.</summary>
            public string Tag { get; init; }
            public string Display { get; init; }
            public override string ToString() => Display;
        }

        private readonly RectangleF _regionScreenPx;
        private readonly byte[] _regionImage;
        private readonly IReadOnlyList<TargetLanguageOption> _targetOptions;
        private readonly IReadOnlyList<SourceLanguageOption> _sourceOptions;
        private readonly Languages _initialTarget;
        private readonly Func<string, Languages, Task<ImageTranslationResult>> _retranslate;

        private ImageTranslationResult _result;
        private Point _dipTopLeft;
        private Point _dipBottomRight;
        private Image _snapshot;
        private bool _suppressEvents;

        public ImageTranslationOverlayWindow(
            RectangleF regionScreenPx,
            byte[] regionImage,
            ImageTranslationResult initialResult,
            IReadOnlyList<TargetLanguageOption> targetOptions,
            IReadOnlyList<SourceLanguageOption> sourceOptions,
            Languages initialTarget,
            Func<string, Languages, Task<ImageTranslationResult>> retranslate)
        {
            InitializeComponent();

            _regionScreenPx = regionScreenPx;
            _regionImage = regionImage;
            _result = initialResult;
            _targetOptions = targetOptions;
            _sourceOptions = sourceOptions;
            _initialTarget = initialTarget;
            _retranslate = retranslate;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _dipTopLeft = PointFromScreen(new Point(_regionScreenPx.X, _regionScreenPx.Y));
            _dipBottomRight = PointFromScreen(new Point(_regionScreenPx.Right, _regionScreenPx.Bottom));

            AddSnapshot();
            PopulateCombos();
            UpdateDetectedLabel();
            RenderBoxes();
        }

        private void AddSnapshot()
        {
            try
            {
                var bitmap = new BitmapImage();
                using var ms = new MemoryStream(_regionImage);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                _snapshot = new Image
                {
                    Source = bitmap,
                    Stretch = Stretch.Fill,
                    Width = Math.Max(1, _dipBottomRight.X - _dipTopLeft.X),
                    Height = Math.Max(1, _dipBottomRight.Y - _dipTopLeft.Y)
                };
                Canvas.SetLeft(_snapshot, _dipTopLeft.X);
                Canvas.SetTop(_snapshot, _dipTopLeft.Y);
                LayerCanvas.Children.Add(_snapshot);
            }
            catch
            {
                // Snapshot is only a visual backdrop; boxes still render without it.
            }
        }

        private void PopulateCombos()
        {
            _suppressEvents = true;

            TargetCombo.ItemsSource = _targetOptions;
            TargetCombo.SelectedItem = _targetOptions.FirstOrDefault(o => o.Value == _initialTarget)
                                       ?? _targetOptions.FirstOrDefault();

            var sources = new List<SourceLanguageOption>
            {
                new SourceLanguageOption { Tag = null, Display = "Auto (detect)" }
            };
            sources.AddRange(_sourceOptions);
            SourceCombo.ItemsSource = sources;
            SourceCombo.SelectedIndex = 0;

            _suppressEvents = false;
        }

        private (double scaleX, double scaleY) ComputeScale()
        {
            var regionW = Math.Max(1, _dipBottomRight.X - _dipTopLeft.X);
            var regionH = Math.Max(1, _dipBottomRight.Y - _dipTopLeft.Y);
            var imgW = _result != null && _result.ImageWidth > 0 ? _result.ImageWidth : regionW;
            var imgH = _result != null && _result.ImageHeight > 0 ? _result.ImageHeight : regionH;

            return (regionW / imgW, regionH / imgH);
        }

        private void RenderBoxes()
        {
            for (var i = LayerCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (LayerCanvas.Children[i] is Border b && (string)b.Tag == "line")
                {
                    LayerCanvas.Children.RemoveAt(i);
                }
            }

            if (_result == null || _result.Lines.Count == 0)
            {
                return;
            }

            var (scaleX, scaleY) = ComputeScale();
            foreach (var line in _result.Lines)
            {
                var x = _dipTopLeft.X + line.Box.X * scaleX;
                var y = _dipTopLeft.Y + line.Box.Y * scaleY;
                var h = Math.Max(1, line.Box.Height * scaleY);

                var textBlock = new TextBlock
                {
                    Text = line.Translation,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xF2, 0xF5, 0xF0)),
                    FontSize = Math.Clamp(h * 0.68, 11, 40),
                    TextWrapping = TextWrapping.Wrap
                };

                var border = new Border
                {
                    Tag = "line",
                    Background = new SolidColorBrush(Color.FromArgb(0xF0, 0x10, 0x14, 0x18)),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(4, 1, 4, 1),
                    MaxWidth = Math.Max(40, _dipBottomRight.X - x),
                    Child = textBlock
                };

                Canvas.SetLeft(border, x);
                Canvas.SetTop(border, y);
                LayerCanvas.Children.Add(border);
            }
        }

        private void UpdateDetectedLabel()
        {
            if (_result == null || !_result.HasText)
            {
                DetectedLabel.Text = "No text detected";
                return;
            }

            DetectedLabel.Text = string.IsNullOrEmpty(_result.DetectedLanguageTag)
                ? "Translated"
                : $"Detected: {_result.DetectedLanguageTag}";
        }

        private async void SourceCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_suppressEvents)
            {
                await RetranslateAsync();
            }
        }

        private async void TargetCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_suppressEvents)
            {
                await RetranslateAsync();
            }
        }

        private async Task RetranslateAsync()
        {
            var target = (TargetCombo.SelectedItem as TargetLanguageOption)?.Value ?? _initialTarget;
            var sourceTag = (SourceCombo.SelectedItem as SourceLanguageOption)?.Tag;

            BusyIndicator.Visibility = Visibility.Visible;
            try
            {
                _result = await _retranslate(sourceTag, target);
            }
            catch
            {
                // Keep the previous result on failure.
            }
            finally
            {
                BusyIndicator.Visibility = Visibility.Collapsed;
            }

            UpdateDetectedLabel();
            RenderBoxes();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_result == null || !_result.HasText)
            {
                return;
            }

            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, _result.Lines.Select(l => l.Translation)));
            }
            catch
            {
                // Clipboard can be transiently locked by another process; ignore.
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void Toolbar_MouseDown(object sender, MouseButtonEventArgs e) => e.Handled = true;

        private void Root_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(Root);
            var insideRegion = pos.X >= _dipTopLeft.X && pos.X <= _dipBottomRight.X
                                                      && pos.Y >= _dipTopLeft.Y && pos.Y <= _dipBottomRight.Y;
            if (!insideRegion)
            {
                Close();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
