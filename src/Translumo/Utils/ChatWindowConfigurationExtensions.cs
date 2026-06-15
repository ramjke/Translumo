using System.Windows.Media;
using Translumo.Configuration;
using Translumo.Infrastructure;

namespace Translumo.Utils
{
    public static class ChatWindowConfigurationExtensions
    {
        private static readonly Brush ErrorTextBrush = new SolidColorBrush(Colors.Red);
        private static readonly Brush InfoTextBrush = new SolidColorBrush(Colors.Aquamarine);

        private static SolidColorBrush _cachedTranslationTextBrush;

        public static Brush GetTextForegroundBrush(this ChatWindowConfiguration configuration, TextTypes textType)
        {
            switch (textType)
            {
                case TextTypes.Translation:
                    return GetTranslationTextBrush(configuration.FontColor);
                case TextTypes.Error:
                    return ErrorTextBrush;
                case TextTypes.Info:
                    return InfoTextBrush;
            }

            return null;
        }

        private static Brush GetTranslationTextBrush(Color brushColor)
        {
            if (_cachedTranslationTextBrush == null || _cachedTranslationTextBrush.Color != brushColor)
            {
                _cachedTranslationTextBrush = new SolidColorBrush(brushColor);

                return _cachedTranslationTextBrush;
            }

            return _cachedTranslationTextBrush;
        }
    }
}
