using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.Data.Text;
using Translumo.Utils;

namespace Translumo.Processing.TextProcessing
{
    public class TextTokenizer
    {
        private readonly WordsSegmenter _segmenter;

        public TextTokenizer(string languageCode)
        {
            _segmenter = new WordsSegmenter(languageCode);
        }

        public string Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var splitted = SplitByDelimiters(text)
                .SelectMany(str =>
                {
                    var words = _segmenter.GetTokens(str).Select(token => token.Text).ToArray();
                    return words.Any() ? words : new[] { str };
                });

            return string.Join(" ", splitted.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private string[] SplitByDelimiters(string text)
        {

            var result = new List<string>();
            var cur = new StringBuilder();
            var lastIsSeparator = false;
            var lastSeparator = ' ';
            void TryAddToResult(StringBuilder str)
            {
                if (str.Length > 0)
                {
                    result.Add(str.ToString());
                    str.Clear();
                }
            }

            foreach (var symb in text)
            {
                var cat = char.GetUnicodeCategory(symb);
                if (cat is >= UnicodeCategory.SpaceSeparator and <= UnicodeCategory.ParagraphSeparator)
                {
                    TryAddToResult(cur);
                    lastSeparator = ' ';
                }
                else if (symb.IsSeparator() || symb == '^')
                {
                    if (!lastIsSeparator || ((lastSeparator != '.' && lastSeparator != '-') || lastSeparator != symb))
                    {
                        TryAddToResult(cur);
                    }

                    cur.Append(symb);
                    lastIsSeparator = true;
                    lastSeparator = symb;
                }
                else
                {
                    if (lastIsSeparator)
                    {
                        TryAddToResult(cur);
                    }

                    cur.Append(symb);
                    lastIsSeparator = false;
                    lastSeparator = ' ';
                }
            }

            TryAddToResult(cur);

            return result.ToArray();
        }
    }
}
