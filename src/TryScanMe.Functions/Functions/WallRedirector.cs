using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Net.Http.Headers;
using TryScanMe.Functions.Repository;
using TryScanMe.Functions.Entities;
using System;

namespace TryScanMe.Functions
{
    public static class WallRedirector
    {
        private static TraceWriter _log;
        private static readonly string _host = Environment.GetEnvironmentVariable("hostname");

        private const string RedirectPath = "/#/wall/";

        [FunctionName("WallRedirector")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Wall/{wallId}")]HttpRequestMessage req, TraceWriter log, string wallId)
        {
            log.Info("Wall scanned: " + wallId);

            _log = log;

            if (wallId == null || wallId.Count() <= 0) return req.CreateErrorResponse(HttpStatusCode.OK, "Ain't no wall here");

            switch (req.Method.ToString()) {
                case "GET":
                    return AWallOrError(wallId);
                default:
                    return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
            }
        }
        private static HttpResponseMessage AWallOrError(string wallId)
        {
            var result = TableStorage.GetEntity<TemporaryUrl>(Constants.TableNames.TemporaryUrl, wallId, wallId);

            if (result == null)
            {
                var path = string.Format($"{wallId}/wall");

                if (!BlobStorage.BlobExists(Constants.BlobContainerNames.Wall, path))
                    return new HttpResponseMessage(HttpStatusCode.NotFound);

                var tempUrl = CreateTemporaryUrl(wallId);

                _log.Info("Wall redirected to: " + wallId);
                
                string redirectPath = RedirectPath + tempUrl;

                var response = new HttpResponseMessage(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri(new Uri(_host), redirectPath);

                return response;
            }
            else
            {
                if (result.Expiry >= DateTime.UtcNow)
                {
                    var html = BlobStorage.GetBlobText("$web", "index.html");

                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(html)
                    };

                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                    return response;
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        Content = new StringContent("Fuck off") // Brutal. Consider revising.
                    };
                }
            }
        }

        private static string CreateTemporaryUrl(string wallId)
        {
            var tempUrl = new TemporaryUrl(wallId);

            TableStorage.Insert(Constants.TableNames.TemporaryUrl, tempUrl);

            return tempUrl.PartitionKey;
        }
    }
}
