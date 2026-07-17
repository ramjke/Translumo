using System;
using Translumo.Infrastructure;
using Translumo.Processing.Interfaces;
using Translumo.Utils;

namespace Translumo.Services
{
    public class ChatUITextMediator : IChatTextMediator
    {
        public event EventHandler<TranslatedEventArgs> TextRaised;
        public event EventHandler ClearTextsRaised; 
        public event EventHandler<TranslatedWithOriginalEventArgs> TextWithOriginalRaised;

        public void SendText(string text, bool successful)
        {
            TextRaised?.RaiseOnUIThread(this, new TranslatedEventArgs(text, successful ? TextTypes.Translation : TextTypes.Error));
        }

        public void SendText(string text, TextTypes textType)
        {
            TextRaised?.RaiseOnUIThread(this, new TranslatedEventArgs(text, textType));
        }

        public void ClearTexts()
        {
            ClearTextsRaised?.RaiseOnUIThread(this);
        }

        public void SendText(string original, string translated)
        {
            TextWithOriginalRaised?.RaiseOnUIThread(this, new TranslatedWithOriginalEventArgs(original, translated, TextTypes.Translation));
        }
    }
}
