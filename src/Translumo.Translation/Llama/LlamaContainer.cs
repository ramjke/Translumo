using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Llama
{
    public sealed class LlamaContainer : TranslationContainer
    {
        public LlamaContainer(Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
        {
        }
    }
}
