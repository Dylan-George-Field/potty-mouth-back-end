using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using TryScanMe.Functions.Extensions;
using TryScanMe.Functions.Models;
using TryScanMe.Functions.Security;

namespace TryScanMe.Functions.Functions.Generate
{
    public static class GenerateSample
    {
        private const string path = "wall/";
        private const string MediaType = "image/jpeg";
        private const string HeaderName = "X-WallId";
        private const string LOGO_DIR = "\\Assets\\logo.png";

        [FunctionName("Sample")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/sample")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            var host = req.GetHost();

            var queryLogo = req.GetQueryValue("logo");

            var isBool = Boolean.TryParse(queryLogo, out bool hasLogo);

            if (!isBool && queryLogo != null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var generator = new UrlGenerator(new Uri(host + path), 1);

            var uri = generator.ToUri().First();

            var bitmap = new QrCode().GenerateImage(uri, 50);

            var wallId = generator.Guids.First().ToString("N");

            byte[] hmac;

            var key = SampleAESKey.Key;

            hmac = AES.EncryptStringToBytes_Aes(wallId, key);

            var ms = new MemoryStream();

            if (hasLogo)
            {
                var logoPath = context.FunctionAppDirectory + LOGO_DIR;
                var logo = new Logo(logoPath);

                ms.Position = 0;

                var qrCodeWithLogo = logo.Image.AppendImage(Image.FromStream(ms), 100, 100);
                ms = new MemoryStream();
                qrCodeWithLogo.Save(ms, ImageFormat.Jpeg);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(ms.ToArray());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
            response.Headers.Add("Access-Control-Expose-Headers", HeaderName);
            response.Headers.Add(HeaderName, AES.ByteToString(hmac));

            return response;
        }
    }
}
