using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;

namespace TryScanMe.Functions.Functions
{
    public static class Dashboard
    {
        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated;

        [FunctionName("Dashboard")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/dashboard")]HttpRequestMessage request, TraceWriter log)
        {
            log.Info("Is Authenticated: " + IsAuthenticated);

            if (!IsAuthenticated) return request.CreateErrorResponse(HttpStatusCode.Forbidden, "These are not the droids you're looking for");

            auth = request.GetAuthInfoAsync(log).Result;

            var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;

            var trackedWalls = TableStorage.QueryByPartitionKey<Tracked>(Constants.TableNames.Tracked, nameId);

            List<Walls> walls = new List<Walls>();

            foreach (var wall in trackedWalls)
            {
                var entity = TableStorage.GetEntity<Walls>(Constants.TableNames.Walls, wall.RowKey, wall.RowKey);

                if (entity.Deleted != true)
                    walls.Add(entity);
            }

            var json = JsonConvert.SerializeObject(walls);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
        }
    }
}
