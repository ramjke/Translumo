using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Translumo.Utils.Http
{
    public class HttpReader
    {
        public CookieContainer Cookies { get; set;}
        public string? Referer { get; set; }
        public bool ThrowExceptions { get; set; } = true;
        public IDictionary<string, string> OptionalHeaders { get; set; }

        public string? UserAgent { get; set; } = "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
        public string? Accept { get; set; } = "text/html, application/xhtml+xml, */*";
        public string? ContentType { get; set; } = "application/x-www-form-urlencoded";
        public WebProxy? Proxy { get; set; }

        protected string? AuthorizationString;


        public HttpReader() : this(null, null)
        {
        }
        
        public HttpReader(CookieContainer cookieIn) : this(cookieIn, null)
        {
        }

        private HttpReader(string authorizationString) : this(null, authorizationString)
        {
        }

        private HttpReader(CookieContainer? cookieIn, string? authorizationString)
        {
            if (cookieIn != null)
            {
                Cookies = cookieIn;
            }
            else
            {
                Cookies = new CookieContainer();
            }
            AuthorizationString = authorizationString;
            OptionalHeaders = new Dictionary<string, string>();
        }

        public virtual async Task<HttpResponse> RequestWebDataAsync(string url, HttpMethods method, bool acceptCookie = false)
        {
            HttpResponse result = new HttpResponse();
            await Task.Run(() => result = RequestWebData(url, method, acceptCookie)).ConfigureAwait(false);
            if (ThrowExceptions && result.InnerException != null)
            {
                throw result.InnerException;
            }

            return result;
        }

        public virtual async Task<HttpResponse> RequestWebDataAsync(string url, HttpMethods method, string dataIn, bool acceptCookie = false)
        {
            HttpResponse result = new HttpResponse();
            await Task.Run(() => result = RequestWebData(url, method, dataIn, acceptCookie)).ConfigureAwait(false);
            if (ThrowExceptions && result.InnerException != null)
            {
                throw result.InnerException;
            }

            return result;
        }

        public virtual HttpResponse RequestWebData(string url, HttpMethods method, bool acceptCookie = false)
        {
            return RequestWebDataInternal(url, method, null, acceptCookie);
        }

        public virtual HttpResponse RequestWebData(string url, HttpMethods method, string dataIn, bool acceptCookie = false)
        {
            return RequestWebDataInternal(url, method, dataIn, acceptCookie);
        }

        protected virtual HttpResponse RequestWebDataInternal(string url, HttpMethods method, string dataIn, bool acceptCookie)
        {
            HttpResponse httpResponse = new HttpResponse();
            try
            {
                WebRequest request = PrepRequest(url, method, Cookies, AuthorizationString);
                string body = ReadWebData(request, dataIn, acceptCookie);
                
                return new HttpResponse(isSuccessful: true, body);
            }
            catch (Exception ex)
            {
                if (ThrowExceptions)
                {
                    throw ex;
                }
                return new HttpResponse(isSuccessful: false, null, ex);
            }
        }

        protected virtual WebRequest PrepRequest(string url, HttpMethods method, CookieContainer? cookie = null, string? authorizationString = null)
        {
            Uri uri = new Uri(url);
            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Method = method.ToString();
            HttpWebRequest httpWebRequest = (HttpWebRequest)webRequest;
            httpWebRequest.Proxy = Proxy;

            if (UserAgent != null)
            {
                httpWebRequest.UserAgent = UserAgent;
            }

            if (Accept != null)
            {
                httpWebRequest.Accept = Accept;
            }

            httpWebRequest.Host = uri.Host;
            if (ContentType != null)
            {
                webRequest.ContentType = ContentType;
            }

            if (Referer != null)
            {
                httpWebRequest.Referer = Referer;
            }

            if (cookie != null)
            {
                httpWebRequest.CookieContainer = cookie;
            }

            if (authorizationString != null)
            {
                httpWebRequest.Headers.Add("Authorization", authorizationString);
            }

            foreach (KeyValuePair<string, string> optionalHeader in OptionalHeaders)
            {
                if (optionalHeader.Key != null && optionalHeader.Value != null)
                {
                    httpWebRequest.Headers.Add(optionalHeader.Key, optionalHeader.Value);
                }
            }

            return webRequest;
        }

        protected virtual string ReadWebData(WebRequest request, string? dataIn, bool acceptCookie)
        {
            string result = string.Empty;
            if (dataIn != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(dataIn);
                request.ContentLength = bytes.Length;
                using Stream stream = request.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            using WebResponse webResponse = request.GetResponse();
            using (Stream stream2 = webResponse.GetResponseStream())
            {
                Encoding encoding = Encoding.GetEncoding("utf-8");
                using (StreamReader streamReader = new StreamReader(stream2, encoding))
                {
                    result = streamReader.ReadToEnd();
                    streamReader.Close();
                }
                if (acceptCookie)
                {
                    try
                    {
                        CookieCollection? cookieCollection = (webResponse as HttpWebResponse)?.Cookies;
                        if (cookieCollection != null)
                        {
                            foreach (Cookie item in cookieCollection)
                            {
                                Cookies.Add(request.RequestUri, item);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                stream2.Close();
            }
            webResponse.Close();

            return result;
        }
    }
}
