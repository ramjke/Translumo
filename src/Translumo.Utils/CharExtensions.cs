using System.Globalization;

namespace Translumo.Utils
{
    public static class CharExtensions
    {
        public static bool IsSeparator(this char c)
        {
            var category = char.GetUnicodeCategory(c);
            return (category is >= UnicodeCategory.ConnectorPunctuation and <= UnicodeCategory.OtherPunctuation) ||
                   category == UnicodeCategory.MathSymbol || category == UnicodeCategory.CurrencySymbol || category == UnicodeCategory.OtherSymbol;
        }
    }
}
