using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Helpers
{
    public static class HttpHelper
    {
        public static HttpRequestMessage Get(this HttpRequestMessage message, string uri)
        {
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(uri);
            return message;
        }

        public static HttpRequestMessage WithHeader(this HttpRequestMessage message, string name, string value)
        {
            message.Headers.Add(name, value);
            return message;
        }
    }
}
