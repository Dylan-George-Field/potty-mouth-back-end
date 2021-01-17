using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TryScanMe.Functions.Auth;
using TryScanMe.Functions.Entities;

namespace TryScanMe.Functions.Functions
{
    public static class Delete
    {
        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("DeleteWall")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/wall/{wallId}")]HttpRequestMessage request, TraceWriter log, string wallId)
        {
            var forbidden = request.CreateErrorResponse(HttpStatusCode.Forbidden, "These are not the droids you're looking for");

            if (!IsAuthenticated) return forbidden;

            auth = request.GetAuthInfoAsync(log).Result;

            var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;

            var isAdmin = TableStorage.GetEntity<Tracked>(Constants.TableNames.Tracked, nameId, wallId);

            if (isAdmin == null) return forbidden;

            var entity = TableStorage.GetEntity<Walls>(Constants.TableNames.Walls, wallId, wallId);

            entity.Deleted = true;

            TableStorage.Insert(Constants.TableNames.Walls, entity);

            var trackedWalls = TableStorage.QueryByPartitionKey<Tracked>("Tracked", nameId);

            List<Walls> walls = new List<Walls>();

            foreach (var wall in trackedWalls)
            {
                var result = TableStorage.GetEntity<Walls>(Constants.TableNames.Walls, wall.RowKey, wall.RowKey);

                if (result.Deleted != true)
                    walls.Add(result);
            }

            var json = JsonConvert.SerializeObject(walls);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
        }
    }
}
