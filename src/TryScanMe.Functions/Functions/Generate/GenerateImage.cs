using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Headers;
using TryScanMe.Functions.Models;
using Newtonsoft.Json;
using TryScanMe.Functions.Entities;
using System.Linq;
using System;
using TryScanMe.Functions.Repository;
using TryScanMe.Functions.Extensions;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Security.Claims;
using TryScanMe.Functions.Auth;

namespace TryScanMe.Functions
{
    public static class GenerateImage
    {
        private const string FILENAME = "tryscanme-image.jpg";
        private const string MEDIA_TYPE = "image/jpeg";
        private const string CONTENT_DISPOSITION_TYPE = "inline";
        private const string logoLocation = "\\Assets\\potty-mouth.png";
        private const string footerLocation = "\\Assets\\potty-mouth-footer.png";

        private const string UrlSuffix = "/wall/";
        private static readonly string QrCodeUrl = Environment.GetEnvironmentVariable("hostname") + UrlSuffix;

        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("GenerateImage")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/Generate/Image")]HttpRequestMessage request, TraceWriter log, ExecutionContext context)
        {
            if (IsAuthenticated)
                auth = request.GetAuthInfoAsync(log).Result;

            log.Info("Is Authenticated: " + IsAuthenticated);

            //We can zip a bunch of them
            var generator = new UrlGenerator(new Uri(QrCodeUrl), 1);
            var guid = generator.Guids.First();

            //Create Blob/Table references
            var wallPath = string.Format($"{guid.ToString("N")}/wall");

            var wallblob = BlobStorage.GetBlob(Constants.BlobContainerNames.Wall, wallPath);

            var contents = new WallEntity(new Message() { Text = Constants.Default.Wall.WelcomeMessage, Username = Constants.Default.Wall.User });

            var json = JsonConvert.SerializeObject(contents);

            wallblob.UploadText(json);

            if (IsAuthenticated)
            {
                var name = auth.GetClaim(ClaimTypes.Name).Value;
                var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;
                var user = new Users(name, nameId);
                TableStorage.Insert(Constants.TableNames.Users, user);

                var trackedWall = new Tracked(nameId, guid.ToString("N"));
                TableStorage.Insert(Constants.TableNames.Tracked, trackedWall);
            }

            var urls = generator.ToUri();
            
            var bitmap = new QrCode().GenerateImage(urls.First(), 10);

            var ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);

            var logoPath = context.FunctionAppDirectory + logoLocation;
            var footerPath = context.FunctionAppDirectory + footerLocation;
            var logo = new Logo(logoPath);
            var footer = new Logo(footerPath);

            ms.Position = 0;

            var qrCodeWithLogo = logo.Image.AppendImage(Image.FromStream(ms), 10, 10);

            ms.Position = 0;

            var footerImage = qrCodeWithLogo.AppendImage(footer.Image, 0, 0);

            var result = footerImage.WriteText(guid.ToString("N").Substring(0, 5), new PointF(40, footerImage.Height - 160));

            var msreadathon = new MemoryStream();

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            result.Save(msreadathon, jpgEncoder, myEncoderParameters);

            //Save image to blob
            var qrCodePath = string.Format($"{guid.ToString("N")}/qrCode");

            var container = BlobStorage.GetContainer(Constants.BlobContainerNames.Wall);

            var qrCodeBlob = container.GetBlockBlobReference(qrCodePath);

            qrCodeBlob.Properties.ContentType = "image/png";

            ms.Position = 0;

            qrCodeBlob.UploadFromStream(ms);

            log.Info("Image uploaded to: " + qrCodeBlob.Uri.AbsoluteUri);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(msreadathon.ToArray());
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(CONTENT_DISPOSITION_TYPE);
            response.Content.Headers.ContentDisposition.FileName = FILENAME;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MEDIA_TYPE);
            return response;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
