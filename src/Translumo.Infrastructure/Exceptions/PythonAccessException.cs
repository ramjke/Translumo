using System;

namespace Translumo.Infrastructure.Exceptions
{
    public class PythonAccessException : Exception
    {
        public PythonAccessException(string message) : base(message)
        {

        }

        public PythonAccessException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
