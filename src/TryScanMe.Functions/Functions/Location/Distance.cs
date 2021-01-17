using System.Net;
using System.Net.Http;
using GeoCoordinatePortable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using TryScanMe.Functions.Extensions;

namespace TryScanMe.Functions.Functions.Location
{
    public static class Distance
    {
        [FunctionName("Distance")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/distance")]HttpRequestMessage req, TraceWriter log)
        {
            var queryClientLatitude = req.GetQueryValue("clientLatitude");

            var queryClientLongtitude = req.GetQueryValue("clientLongitude");

            var queryQrcodeLatitude = req.GetQueryValue("qrcodeLatitude");

            var queryQrcodeLongtitude = req.GetQueryValue("qrcodeLongitude");

            var isClientLatitude = double.TryParse(queryClientLatitude, out double clientLatitude);

            var isClientLongtitude = double.TryParse(queryClientLongtitude, out double clientLongtitude);

            var isQrcodeLatitude = double.TryParse(queryQrcodeLatitude, out double qrcodelatitude);

            var isQrcodeLongtitude = double.TryParse(queryQrcodeLongtitude, out double qrcodeLongitude);

            if (!isClientLatitude || !isClientLongtitude || !isQrcodeLatitude || !isQrcodeLongtitude
                || !ValidateLatitude(clientLatitude) || !ValidateLatitude(qrcodelatitude) 
                || !ValidateLongitude(clientLongtitude) || !ValidateLongitude(qrcodeLongitude))
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var myCoord = new GeoCoordinate(clientLatitude, clientLongtitude);
            var wallCoord = new GeoCoordinate(qrcodelatitude, qrcodeLongitude);

            //Haversine formula, result in meters
            var distance = myCoord.GetDistanceTo(wallCoord);

            return req.CreateResponse(HttpStatusCode.OK, new { distance });
        }

        private static bool ValidateLatitude(double latitude)
        {
            return latitude >= -90 && latitude <= 90;
        }
        
        public static bool ValidateLongitude(double longitude)
        {
            return longitude >= -180 && longitude <= 180;
        }
    }
}
