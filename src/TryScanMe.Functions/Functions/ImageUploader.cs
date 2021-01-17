using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;
using TryScanMe.Functions.Repository;

namespace TryScanMe.Functions.Functions
{
    public static class ImageUploader
    {
        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        private const string DateTimeFormat = "yyyy-MM-dd-HH-mm-ss-ff";

        [FunctionName("UploadImage")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/image/{wallId}")]
                HttpRequestMessage req, string wallId, TraceWriter log)
        {
            if (IsAuthenticated)
                auth = req.GetAuthInfoAsync(log).Result;

            log.Info("Is Authenticated: " + IsAuthenticated);

            if (!req.Content.IsMimeMultipartContent())
                return new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);

            var multipart = await req.Content.ReadAsMultipartAsync();

            var content = multipart.Contents.First();

            var stream = await content.ReadAsStreamAsync();

            var filename = "no-filename";

            if (content.Headers.ContentDisposition.FileName != null)
            {
                filename = content.Headers.ContentDisposition.FileName.Replace("\"", "");
            }

            if (content.Headers.ContentLength == 0)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            if (filename.Length > 500)
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("filename above 500 characters")
                };

            Image image;

            try
            {
                image = Image.FromStream(stream);
            }
            catch (Exception )
            {
                log.Error("An invalid filetype was attempted to be uploaded.");
                return new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);

            }               

            var result = TableStorage.GetEntity<TemporaryUrl>(Constants.TableNames.TemporaryUrl, wallId, wallId);

            if (result == null) return new HttpResponseMessage(HttpStatusCode.NotFound);

            if (result.Expiry <= DateTime.UtcNow)
                return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("Fuck Off") };

            var container = BlobStorage.GetContainer(Constants.BlobContainerNames.Image);

            //Allow public access to this container - 'images'
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });

            var folder = result.RealUrl;

            var prefix = DateTime.UtcNow.ToString(DateTimeFormat) + "_";

            var path = string.Format($"{folder}/{prefix}{filename}");

            var blob = container.GetBlockBlobReference(path);

            blob.Properties.ContentType = content.Headers.ContentType.ToString();

            stream.Position = 0;

            blob.UploadFromStream(stream);

            //Get Blob Url and add it to the post.

            var uri = blob.Uri.AbsoluteUri;

            //This is copied in Wall (text)

            var name = "";
            var nameId = "";

            if (IsAuthenticated && auth != null)
            {
                name = auth.GetClaim(ClaimTypes.GivenName).Value;
                nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;
            }

            log.Info("Name: " + name);

            var wall = BlobStorage.GetWall(result.RealUrl);

            var wallMessage = new Message { ImageUri = uri, Filename = filename, Username = name, NameId = nameId };

            wall.Messages.Add(wallMessage);

            var block = BlobStorage.GetBlob(Constants.BlobContainerNames.Wall, $"{result.RealUrl}/wall");

            var json = JsonConvert.SerializeObject(wall);

            block.UploadText(json);

            wallMessage.SendToSlack(result.RealUrl);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
        }
    }
}
