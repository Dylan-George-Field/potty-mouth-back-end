using System.Net;
using System.Net.Http;
using System.Security.Claims;
using GeoCoordinatePortable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;
using TryScanMe.Functions.Extensions;

namespace TryScanMe.Functions.Functions.Location
{
    public static class IsWithinDistance
    {
        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("IsWithinDistance")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/{wallId}/IsWithinDistance")]HttpRequestMessage request, TraceWriter log, string wallId)
        {
            if (IsAuthenticated)
                auth = request.GetAuthInfoAsync(log).Result;

            log.Info("Is Authenticated: " + IsAuthenticated);

            var queryLatitude = request.GetQueryValue("latitude");

            var queryLongtitude = request.GetQueryValue("longitude");

            var isLatitude = double.TryParse(queryLatitude, out double latitude);

            var isLongtitude = double.TryParse(queryLongtitude, out double longitude);

            if (!isLatitude || !isLongtitude || latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            log.Info("Client\n Latitude: " + latitude + "\nLongitude: " + longitude);

            bool isAdmin = false;
            string realWallId = wallId;

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

            var wall = TableStorage.GetEntity<Walls>(Constants.TableNames.Walls, realWallId, realWallId);

            if (wall.Latitude == 0 && wall.Longitude == 0)
                return request.CreateResponse(HttpStatusCode.OK, new { IsGeolocated = false, Message = "There is no location data for this wall" });

            log.Info("QR Code\n Latitude: " + wall.Latitude + "\nLongitude: " + wall.Longitude);

            var myCoord = new GeoCoordinate(latitude, longitude);
            var wallCoord = new GeoCoordinate(wall.Latitude, wall.Longitude);

            //Haversine formula
            var distance = myCoord.GetDistanceTo(wallCoord);

            log.Info("Distance from client to QR code: " + distance + " meters");

            if (distance > wall.MaxDistance && !isAdmin)
                return request.CreateResponse(HttpStatusCode.OK, new { IsGeolocated = true, IsWithinDistance = false, Message = "You're outside the geo-location" });

            //Todo, consider revising the way 'IsAdmin' works in the entire application
            if (distance > wall.MaxDistance && isAdmin)
                return request.CreateResponse(HttpStatusCode.OK, new { IsAdmin = true, IsGeolocated = true, IsWithinDistance = false, Distance = distance, wall.MaxDistance, Message = "You're outside the geo-location" });

            if (isAdmin)
                return request.CreateResponse(HttpStatusCode.OK, new { IsAdmin = true, IsGeolocated = true, IsWithinDistance = true, Distance = distance, wall.MaxDistance });

            return request.CreateResponse(HttpStatusCode.OK, new { IsGeolocated = true, IsWithinDistance = true });
        }
    }
}
