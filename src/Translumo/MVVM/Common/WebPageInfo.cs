using System.Collections.Generic;
using System.Net;
using Microsoft.Web.WebView2.Core;

namespace Translumo.MVVM.Common
{
    public class WebPageInfo
    {
        public IEnumerable<Cookie> Cookies { get; set; }

        public string Body { get; set; }
    }
}
