using System;
using Translumo.Infrastructure;

namespace Translumo.MVVM.Models
{
    public class ChatItemAddedEventArgs : EventArgs
    {
        public string Text { get; set; }

        public TextTypes TextType { get; set; }

        public ChatItemAddedEventArgs(string text, TextTypes textType)
        {
            this.Text = text;
            this.TextType = textType;
        }
    }
}
