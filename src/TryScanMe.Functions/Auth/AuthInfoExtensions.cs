using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

//https://github.com/stuartleeks/AzureFunctionsEasyAuth/blob/master/src/FunctionWithAuth/AuthInfoExtensions.cs
namespace TryScanMe.Functions.Auth
{
    public static class AuthInfoExtensions
    {
        private static HttpClient _httpClient = new HttpClient(); // cache and reuse to avoid repeated creation on Function calls

        /// <summary>
        /// Find a claim of the specified type
        /// </summary>
        /// <param name="authInfo"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public static AuthUserClaim GetClaim(this AuthInfo authInfo, string claimType)
        {
            return authInfo.UserClaims.FirstOrDefault(c => c.Type == claimType);
        }

        /// <summary>
        /// Get the EasyAuth properties for the currently authenticated user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<AuthInfo> GetAuthInfoAsync(this HttpRequestMessage request, TraceWriter log)
        {
            string zumoAuthToken = request.GetZumoAuthToken();
            log.Info(zumoAuthToken);
            if (string.IsNullOrEmpty(zumoAuthToken))
            {
                return null;
            }
            var authMeRequest = new HttpRequestMessage(HttpMethod.Get, GetEasyAuthEndpoint())
            {
                Headers =
                        {
                            { "X-ZUMO-AUTH", zumoAuthToken }
                        }
            };
            var response = await _httpClient.SendAsync(authMeRequest);
            var authInfo = await response.Content.ReadAsStringAsync();
            log.Info(authInfo);
            var result = JsonConvert.DeserializeObject<List<AuthInfo>>(authInfo);
            return result.First() ?? null;
        }
        private static string GetEasyAuthEndpoint()
        {
            // Get the hostname from environment variables so that we don't need config - thank you App Service!
            var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            // Build up the .auth/me url
            string requestUri = $"https://dylans-function.azurewebsites.net/.auth/me";
            return requestUri;
        }
        private static string GetZumoAuthToken(this HttpRequestMessage req)
        {
            var result = req.Headers.TryGetValues("X-ZUMO-AUTH", out IEnumerable<string> values);

            return result == true ? values.FirstOrDefault() : null;
        }
    }
}
