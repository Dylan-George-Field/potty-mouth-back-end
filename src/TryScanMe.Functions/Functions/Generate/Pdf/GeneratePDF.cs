using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Net.Http.Headers;
using TryScanMe.Functions.Models;
using Newtonsoft.Json;
using TryScanMe.Functions.Repository;
using TryScanMe.Functions.Entities;
using System;
using TryScanMe.Functions.Extensions;
using TryScanMe.Functions.Functions.Generate.pdf;
using PdfSharp.Pdf;
using System.Collections.Generic;
using System.Linq;

namespace TryScanMe.Functions
{
    public static class GeneratePdf
    {
        private const string Filename = "TryScanMe-print-8.pdf";
        private const string MediaType = "application/pdf";
        private const string ContentDispositionType = "inline";

        private const string UrlSuffix = "/wall/";
        private static readonly string QrCodeUrl = Environment.GetEnvironmentVariable("hostname") + UrlSuffix;

        [FunctionName("GeneratePdf")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/generate/pdf")]
                HttpRequestMessage request, ExecutionContext context, TraceWriter log)
        {
            var data = await request.Content.ReadAsAsync<RequestBody>();

            var qGuid = request.GetQueryValue("guid");
            var qNumber = request.GetQueryValue("number");
            var qOffset = request.GetQueryValue("offset");
            var qType = request.GetQueryValue("type");

            var query = new List<bool> {
                int.TryParse(qNumber, out int number),
                int.TryParse(qOffset, out int offset),
                Enum.TryParse(qType, out LabelTypes type)
            };

            if (!IsValid(query, number, offset, type))
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

            Guid guidToCopy = Guid.Empty;

            if (!string.IsNullOrEmpty(qGuid))
                // Contains no bracks {} or hyphens -
                query.Add(Guid.TryParseExact(qGuid.ToString(), "N", out guidToCopy));

            UrlGenerator generator;

            generator = (guidToCopy == Guid.Empty)
                ? new UrlGenerator(new Uri(QrCodeUrl), number)
                : new UrlGenerator(new Uri(QrCodeUrl), number, guidToCopy);

            generator.Guids.ForEach(x => CreateWallEntry(x));

            generator.Guids.ForEach(x => log.Info("Wall created: " + x.ToString()));

            var document = new PdfDocument();
            var page = document.AddPage();

            switch (type)
            {
                case LabelTypes.DL8:
                    page.ConvertToDL8(generator, offset, context.FunctionAppDirectory);
                    break;
                case LabelTypes.DL16:
                    page.ConvertToDL16(generator, offset, context.FunctionAppDirectory);
                    break;
                default:
                    return new HttpResponseMessage(HttpStatusCode.NotImplemented);
            }

            var stream = new MemoryStream();
            document.Save(stream, false);
            stream.Position = 0;

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(ContentDispositionType);
            response.Content.Headers.ContentDisposition.FileName = Filename;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);
            return response;
        }

        private static bool IsValid(List<bool> query, int numberToGenerate, int offset, LabelTypes numberOfLabels)
        {
            return query.All(x => x == true)
                || offset < 0
                || numberToGenerate <= 0
                || offset > (int)numberOfLabels - 1
                || numberToGenerate > (int)numberOfLabels;

        }

        private static void CreateWallEntry(Guid guid)
        {
            var path = string.Format($"{guid.ToString("N")}/wall");

            var block = BlobStorage.GetBlob(Constants.BlobContainerNames.Wall, path);

            var wall = JsonConvert.SerializeObject(new WallEntity(new Message { Text = Constants.Default.Wall.WelcomeMessage }));

            block.UploadTextAsync(wall);
        }
    }
}
