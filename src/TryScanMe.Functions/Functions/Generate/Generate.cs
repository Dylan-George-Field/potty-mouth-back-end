using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;
using TryScanMe.Functions.Extensions;
using TryScanMe.Functions.Repository;
using TryScanMe.Functions.Security;

namespace TryScanMe.Functions.Functions.Generate
{
    public static class Generate
    {
        private const string path = "wall/";

        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("Generate")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/generate")]HttpRequestMessage req, TraceWriter log)
        {
            if (IsAuthenticated)
                auth = req.GetAuthInfoAsync(log).Result;

            log.Info("Is Authenticated: " + IsAuthenticated);

            var queryTracked = req.GetQueryValue("tracked");

            var title = req.GetQueryValue("title");

            var queryLogo = req.GetQueryValue("logo");

            var queryLatitude = req.GetQueryValue("latitude");

            var queryLongitude = req.GetQueryValue("longitude");

            var queryWallToken = req.GetQueryValue("wallToken");

            var wallToken = DecryptWallToken(queryWallToken);

            var isTracked = Boolean.TryParse(queryTracked, out bool tracked);

            var logoParsed = Boolean.TryParse(queryTracked, out bool hasLogo);

            var isLatitude = double.TryParse(queryLatitude, out double latitude);

            var isLongitude = double.TryParse(queryLongitude, out double longitude);

            var isWallId = Guid.TryParse(wallToken, out Guid wallId);

            //There's no validation on the values
            if (!isTracked || !logoParsed || (!isLatitude && queryLatitude != null) || (!isLongitude && queryLongitude != null) || !isWallId)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var wallPath = string.Format($"{wallId.ToString("N")}/wall");

            var wallblob = BlobStorage.GetBlob(Constants.BlobContainerNames.Wall, wallPath);

            var contents = new WallEntity(new Message { Text = Constants.Default.Wall.WelcomeMessage, Username = Constants.Default.Wall.User });

            var json = JsonConvert.SerializeObject(contents);

            wallblob.UploadText(json);

            var wall = new Walls(wallId.ToString("N"), latitude, longitude, title);

            TableStorage.Insert(Constants.TableNames.Walls, wall);

            var host = req.GetHost();

            var generator = new UrlGenerator(new Uri(host + path), 1);

            var uri = generator.ToUri().First();

            var bitmap = new QrCode().GenerateImage(uri, 50);
            
            var qrCodePath = string.Format($"{wallId.ToString("N")}/qrCode");

            var container = BlobStorage.GetContainer(Constants.BlobContainerNames.Wall);

            var qrCodeBlob = container.GetBlockBlobReference(qrCodePath);

            qrCodeBlob.Properties.ContentType = "image/jpeg";

            var ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Jpeg);

            qrCodeBlob.UploadFromStream(ms);

            log.Info("Image uploaded to: " + qrCodeBlob.Uri.AbsoluteUri);

            if (tracked && IsAuthenticated)
            {
                var name = auth.GetClaim(ClaimTypes.Name).Value;
                var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;
                var user = new Users(name, nameId);
                TableStorage.Insert(Constants.TableNames.Users, user);

                var trackedWall = new Tracked(nameId, wallId.ToString("N"));
                TableStorage.Insert(Constants.TableNames.Tracked, trackedWall);
            }

            if (tracked)
            {
                return req.CreateResponse(HttpStatusCode.OK, new { Created = DateTime.UtcNow, WallId = wallId.ToString("N") });
            } else
            {
                return req.CreateResponse(HttpStatusCode.OK, new { Created = DateTime.UtcNow });
            }
        }

        private static string DecryptWallToken(string cipherText)
        {
            var hmacByte = AES.StringToByteArray(cipherText);

            return AES.DecryptStringFromBytes_Aes(hmacByte, SampleAESKey.Key);
        }
    }
}
