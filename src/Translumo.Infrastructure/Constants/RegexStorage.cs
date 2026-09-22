using System.Text.RegularExpressions;

namespace Translumo.Infrastructure.Constants
{
    public static class RegexStorage
    {

        public static Regex MultipleSpacesRegex { get; set; }

        public static Regex StartDotRegex { get; set; }

        public static Regex EndDotRegex { get; set; }

        public static Regex GuidGenerationRegex { get; set; }



        static RegexStorage()
        {
            var punctuation = Regex.Escape(";:?!.-");
            
            MultipleSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);
            StartDotRegex = new Regex(@"^\.{3,}", RegexOptions.Compiled);
            EndDotRegex = new Regex(@"\.{3,}$", RegexOptions.Compiled);
            GuidGenerationRegex = new Regex("[xy]", RegexOptions.Compiled);
        }
    }
}
