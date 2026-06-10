using System;
using Translumo.Infrastructure;

namespace Translumo.Services
{
    public class TranslatedEventArgs : EventArgs
    {
        public string Text { get; set; }
        public TextTypes TextType { get; set; }

        public TranslatedEventArgs(string text, TextTypes textType)
        {
            Text = text;
            TextType = textType;
        }
    }
}
