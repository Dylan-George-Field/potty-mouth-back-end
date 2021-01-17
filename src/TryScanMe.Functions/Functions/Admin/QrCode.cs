
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;
using TryScanMe.Functions.Repository;

namespace TryScanMe.Functions.Functions.Admin
{
    public static class QrCode
    {
        private const string FILENAME = "tryscanme-image.jpg";
        private const string MEDIA_TYPE = "image/jpeg";
        private const string CONTENT_DISPOSITION_TYPE = "inline";

        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("QrCode")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/admin/qrCode/{wallId}")]
            HttpRequestMessage request, TraceWriter log, string wallId)
        {
            if (!IsAuthenticated) return request.CreateErrorResponse(HttpStatusCode.Forbidden, "These are not the droids you're looking for");

            auth = request.GetAuthInfoAsync(log).Result;

            log.Info("Is Authenticated: " + IsAuthenticated);

            var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;

            var trackedWalls = TableStorage.QueryByPartitionKey<Tracked>(Constants.TableNames.Tracked, nameId);

            if (trackedWalls == null) return new HttpResponseMessage(HttpStatusCode.NotFound);

            var path = $"{wallId}/qrCode";

            log.Info("Getting blob: " + path);

            var blob = BlobStorage.GetBlob(Constants.BlobContainerNames.Wall, path);

            var ms = new MemoryStream();
            try
            {
                blob.DownloadToStream(ms);
            } catch (WebException e)
            {
                log.Error(e.Message);
                return request.CreateErrorResponse(HttpStatusCode.InternalServerError, "QrCode not found");
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(ms.ToArray());
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(CONTENT_DISPOSITION_TYPE);
            response.Content.Headers.ContentDisposition.FileName = FILENAME;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MEDIA_TYPE);

            return response;
        }
    }
}
