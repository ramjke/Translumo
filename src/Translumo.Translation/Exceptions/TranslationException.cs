using System;

namespace Translumo.Translation.Exceptions
{
    public class TranslationException : Exception
    {
        public TranslationException(string message) : base(message)
        {
        }

        public TranslationException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
        public TranslationException() : base()
        {
        }
    }
}
