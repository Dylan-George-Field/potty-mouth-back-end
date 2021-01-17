using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;
using TryScanMe.Functions.Extensions;

namespace TryScanMe.Functions.Functions.Location
{
    public static class PatchLocation
    {
        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("PatchLocation")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "api/location/{wallId}")]HttpRequestMessage req, TraceWriter log, string wallId)
        {
            log.Info("Is Authenticated: " + IsAuthenticated);

            var body = await req.Content.ReadAsStringAsync();

            var entity = JsonConvert.DeserializeObject<Request>(body);

            if (entity.Latitude < -90 || entity.Latitude > 90 || entity.Longitude < -180 || entity.Longitude > 180 || entity.Distance <= 0)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            auth = req.GetAuthInfoAsync(log).Result;

            var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;

            var result = TableStorage.GetEntity<Walls>(Constants.TableNames.Walls, wallId, wallId);

            if (result == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            result.Latitude = entity.Latitude;
            result.Longitude = entity.Longitude;
            result.MaxDistance = entity.Distance;

            TableStorage.Insert(Constants.TableNames.Walls, result);

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }

    public class Request
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Distance { get; set; }
    }
}
