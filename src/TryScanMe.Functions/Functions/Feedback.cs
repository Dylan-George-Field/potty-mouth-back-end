using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace TryScanMe.Functions
{
    public static class Feedback
    {
        [FunctionName("Feedback")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/feedback")] HttpRequestMessage req, TraceWriter log)
        {
            var body = await req.Content.ReadAsStringAsync();

            FeedbackMessage feedback;

            try
            {
                feedback = JsonConvert.DeserializeObject<FeedbackMessage>(body);
            }
            catch (JsonSerializationException)
            {
                log.Error("Feedback Message did not deserialise. Check the body of the message is correct");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            if ( feedback.Message.Length > 2500 || feedback.Rating < 0 || feedback.Rating > 5 )
            {
                log.Error("Model failed validation. Check the message length and rating fits the validation rules");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            using (var client = new HttpClient())
            {

                var json = JsonConvert.SerializeObject(
                            new
                            {
                                text = feedback.Email + "\n" + feedback.Rating + " star(s)" + "\n" + feedback.Message
                            });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = client.PostAsync(
                    "https://hooks.slack.com/services/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", content).Result;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
    public class FeedbackMessage
    {
        public string Email { get; set; }
        public string Message { get; set; }
        public int Rating { get; set; }
    }
}
