using System;
using System.Windows;
using System.Windows.Media;
using Translumo.MVVM.Models;
using Translumo.Processing.Configuration;
using Translumo.Utils;

namespace Translumo.Configuration
{
    public class ChatWindowConfiguration : BindableBase
    {
        public event EventHandler ExcludeFromCaptueChanged;

        public Color BackgroundColor
        {
            get => _backgroundColor; 
            set
            {
                SetProperty(ref _backgroundColor, value);
            }
        }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                SetProperty(ref _fontColor, value);
            }
        }

        public float BackgroundOpacity
        {
            get => _backgroundOpacity;
            set
            {
                SetProperty(ref _backgroundOpacity, value);
            }
        }

        public int FontSize
        {
            get => _fontSize;
            set
            {
                SetProperty(ref _fontSize, value);
            }
        }

        public bool FontBold
        {
            get => _fontBold;
            set
            {
                SetProperty(ref _fontBold, value);
            }
        }

        public bool ExcludeFromCaptue
        {
            get => _excludeFromCaptue;
            set
            {
                SetProperty(ref _excludeFromCaptue, value);
                // Only raise the event if the property actually changed
                ExcludeFromCaptueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int LineSpacing
        {
            get => _lineSpacing;
            set
            {
                SetProperty(ref _lineSpacing, value);
            }
        }

        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                SetProperty(ref _textAlignment, value);
            }
        }

        [MapInternal]
        public TextProcessingConfiguration TextProcessing
        {
            get => _textProcessing;
            set
            {
                SetProperty(ref _textProcessing, value);
            }
        }

        public static ChatWindowConfiguration Default => new()
        {
            BackgroundColor = Color.FromRgb(0, 0, 0),
            FontColor = Color.FromRgb(255, 255, 255),
            BackgroundOpacity = 0.65f,
            FontSize = 15,
            FontBold = true,
            ExcludeFromCaptue = true,
            LineSpacing = 14,
            TextAlignment = TextAlignment.Left
        };

        private Color _backgroundColor;
        private Color _fontColor;
        private float _backgroundOpacity;
        private int _fontSize;
        private bool _fontBold;
        private bool _excludeFromCaptue;
        private int _lineSpacing;
        private TextProcessingConfiguration _textProcessing = TextProcessingConfiguration.Default;
        private TextAlignment _textAlignment;
    }
}
