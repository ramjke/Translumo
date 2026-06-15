using System;

namespace Translumo.Utils.Http
{
    public class HttpResponse
    {
        public virtual string Body { get; protected set; }

        public virtual bool IsSuccessful { get; protected set; }

        public virtual Exception InnerException { get; protected set; }

        public HttpResponse()
        {
            this.Body = null;
            this.IsSuccessful = false;
            this.InnerException = null;
        }

        public HttpResponse(bool isSuccessful, string body)
        {
            this.Body = body;
            this.IsSuccessful = isSuccessful;
            this.InnerException = null;
        }

        public HttpResponse(bool isSuccessful, string body, Exception exception)
        {
            this.Body = body;
            this.IsSuccessful = isSuccessful;
            this.InnerException = exception;
        }
    }
}
