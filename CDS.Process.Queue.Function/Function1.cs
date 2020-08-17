using System;
using System.Net.Http;
using CDS.Process.Queue.Function.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CDS.Process.Queue.Function
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async void Run([ServiceBusTrigger("cds-queue", Connection = "AzureWebJobsServiceBus")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            var payload = JsonConvert.DeserializeObject<QueueMessage>(myQueueItem);

            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://jsonplaceholder.typicode.com/todos/1");
                HttpResponseMessage response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
