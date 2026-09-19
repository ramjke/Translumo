using System.Text.RegularExpressions;

namespace Translumo.Infrastructure.Constants
{
    public static class RegexStorage
    {

        public static Regex MultipleSpacesRegex { get; set; }

        public static Regex StartDotRegex { get; set; }

        public static Regex EndDotRegex { get; set; }

        public static Regex GoogleTranslateResultRegex { get; set; }

        public static Regex GuidGenerationRegex { get; set; }

        public static Regex YandexSidRegex { get; set; }

        public static Regex DeeplSentenceRegex { get; set; }

        static RegexStorage()
        {
            var punctuation = Regex.Escape(";:?!.-");
            
            MultipleSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);
            StartDotRegex = new Regex(@"^\.{3,}", RegexOptions.Compiled);
            EndDotRegex = new Regex(@"\.{3,}$", RegexOptions.Compiled);
            GoogleTranslateResultRegex = new Regex("(?<=(<div(.*)class=\"result-container\"(.*)>))[\\s\\S]*?(?=(<\\/div>))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            GuidGenerationRegex = new Regex("[xy]", RegexOptions.Compiled);
            YandexSidRegex = new Regex(@"(?<=(sid\:\s*')).*(?=('))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            DeeplSentenceRegex = new Regex(@"(\S.+?([.!?♪。]|$))([\r\n]+|(?=\s+|$))", RegexOptions.Compiled | RegexOptions.Multiline);
        }
    }
}
