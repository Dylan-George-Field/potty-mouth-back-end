using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using TryScanMe.Functions.Entities;

namespace TryScanMe.Functions
{
    public static class MessageExtensions
    {
        public static void SendToSlack(this Message message, string id)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(
                            new
                            {
                                text = id.ToString() + "\n" +
                                       "*" + message.Username + ":* " + ( message.Text ?? "<" + message.ImageUri + "|" + message.Filename + ">" )
                            });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = client.PostAsync(
                    "https://hooks.slack.com/services/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", content).Result;
            }
        }
    }
}
