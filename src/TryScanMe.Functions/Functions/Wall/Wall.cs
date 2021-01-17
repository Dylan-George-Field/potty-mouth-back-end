using System;
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
using TryScanMe.Functions.Repository;

namespace TryScanMe.Functions.Functions
{
    public static class Wall
    {
        private static TraceWriter _log;

        private static AuthInfo auth;
        private static readonly bool IsAuthenticated = ClaimsPrincipal.Current.Identity.IsAuthenticated;

        [FunctionName("Wall")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/Wall/{wallId}")] HttpRequestMessage req, string wallId, TraceWriter log)
        {
            _log = log;

            if (IsAuthenticated)
                auth = req.GetAuthInfoAsync(log).Result;

            log.Info("Is Authenticated: " + IsAuthenticated);

            bool isAdmin = false;
            string realWallId = wallId;

            if (IsAuthenticated)
            {
                var nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;
                var result = TableStorage.GetEntity<Tracked>(Constants.TableNames.Tracked, nameId, wallId);
                isAdmin = result != null;

                log.Info("isAdmin: " + isAdmin);
            }

            var tempTable = TableStorage.GetEntity<TemporaryUrl>(Constants.TableNames.TemporaryUrl, realWallId, realWallId);

            if (tempTable == null && !isAdmin) return new HttpResponseMessage(HttpStatusCode.NotFound);

            if (!isAdmin)
                realWallId = tempTable.RealUrl;

            if (isAdmin || tempTable.Expiry >= DateTime.UtcNow )
            {
                switch (req.Method.ToString())
                {
                    case "GET":
                        return req.CreateResponse(HttpStatusCode.OK, GetWall(realWallId));
                    case "POST":
                        return await UpdateWall(req, realWallId);
                    default:
                        return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
                }
            } else {
                return new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("Fuck Off")
                };
            }
        }
        
        private static object GetWall(string wallId)
        {
            var meta = TableStorage.GetEntity<Walls>(Constants.TableNames.Walls, wallId, wallId);

            string Title = "Untitled Wall";

            if (meta == null)
            {
                _log.Warning($"The metadata for {wallId} in the Wall table was not found and should exist.");

            } else
            {
                Title = meta.Title;
            }

            return new { BlobStorage.GetWall(wallId).Messages, Title };
        }

        private async static Task<HttpResponseMessage> UpdateWall(HttpRequestMessage req, string wallId)
        {
            var name = "";
            var nameId = "";

            if (IsAuthenticated && auth != null)
            {
                name = auth.GetClaim(ClaimTypes.GivenName).Value;
                nameId = auth.GetClaim(ClaimTypes.NameIdentifier).Value;
            }

            var wall = BlobStorage.GetWall(wallId);
            if (wall == null) return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var message = await req.Content.ReadAsStringAsync();

            var wallMessage = new Message { Text = message, Username = name, NameId = nameId };

            wall.Messages.Add(wallMessage);

            var wallPath = string.Format($"{wallId}/wall");

            var block = BlobStorage.GetBlob(Constants.BlobContainerNames.Wall, wallPath);

            var json = JsonConvert.SerializeObject(wall);

            block.UploadText(json);

            wallMessage.SendToSlack(wallId);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            return response;
        }
    }
}
