using System;

namespace Translumo.Services
{
    public class TranslatedWithOriginalEventArgs : EventArgs
    {
        public string Original { get; set; }
        public string Translated { get; set; }
        public Translumo.Infrastructure.TextTypes TextType { get; set; }

        public TranslatedWithOriginalEventArgs(string original, string translated, Translumo.Infrastructure.TextTypes textType)
        {
            Original = original;
            Translated = translated;
            TextType = textType;
        }
    }
}
