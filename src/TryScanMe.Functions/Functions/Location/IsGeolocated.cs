using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;

namespace TryScanMe.Functions.Functions.Location
{
    public class IsGeolocated
    {
        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("IsGeolocated")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/{wallId}/IsGeolocated")]HttpRequestMessage request, TraceWriter log, string wallId)
        {
            if (IsAuthenticated)
                auth = request.GetAuthInfoAsync(log).Result;

            log.Info("Is Authenticated: " + IsAuthenticated);

            bool isAdmin = false;
            string realWallId = wallId;
            //if admin ignore geo location
            if (IsAuthenticated)
            {
                var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;
                var result = TableStorage.GetEntity<Tracked>(Constants.TableNames.Tracked, nameId, wallId);
                isAdmin = result != null;

                log.Info("isAdmin: " + isAdmin);
            }

            var tempTable = TableStorage.GetEntity<TemporaryUrl>(Constants.TableNames.TemporaryUrl, wallId, wallId);

            if (tempTable == null && !isAdmin)
                return request.CreateResponse(HttpStatusCode.NotFound, "There's no wall here");

            if (!isAdmin)
                realWallId = tempTable.RealUrl;

            var location = TableStorage.GetEntity<Walls>(Constants.TableNames.Walls, realWallId, realWallId);

            if (location == null)
                return request.CreateResponse(HttpStatusCode.OK, new { IsGeolocated = false });

            return request.CreateResponse(HttpStatusCode.OK, new { IsGeolocated = true });
        }
    }
}
