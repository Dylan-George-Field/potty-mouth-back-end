using System;
using System.Linq;
using System.Net.Http;

namespace TryScanMe.Functions.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static string GetQueryValue(this HttpRequestMessage request, string value)
        {
            return request.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, value, true) == 0)
                .Value;
        }

        public static Uri GetHost(this HttpRequestMessage request)
        {
            return new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority));
        }
    }
}
