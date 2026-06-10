using System;

namespace Translumo.Infrastructure.Exceptions
{
    public class OptionalFeatureAccessException : Exception
    {
        public OptionalFeatureAccessException(string message) : base(message)
        {
            
        }

        public OptionalFeatureAccessException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
