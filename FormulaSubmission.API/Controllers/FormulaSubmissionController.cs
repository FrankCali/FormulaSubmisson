using FormulaSubmission.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FormulaSubmission.API.Controllers
{
    [ApiController]
    [Route("api/FormulaSubmission")]
    public class FormulaSubmissionController : ControllerBase
    {
        const string QueueName = "cds-queue";
        const string BlobContainerName = "cds-container";

        private IConfiguration _configuration;

        public FormulaSubmissionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{formulaStatusId}", Name="GetFormulaStatus")]
        public IActionResult GetFormulaStatus(Guid formulaStatusId)
        {
            try
            {
                var formulaStatus = GetBlob(BlobContainerName, formulaStatusId.ToString());

                var response = new FormulaStatusResponse
                {
                    FormulaStatus = formulaStatus
                };

                return Ok(response);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
                return NotFound();
            }
        }
        [HttpPost]
        public async Task<ActionResult<SubmitFormulaResponse>> SubmitFormula([FromBody]SubmitFormulaRequest request)
        {
            //Connect to Service Bus and Specify Queue
            var queueClient = new QueueClient(_configuration.GetValue<string>("ServiceBusConnectionString"), QueueName);

            //Create message to send to Service Bus Queue
            var messageToQueue = new QueueMessage
            {
                FormulaStatusId = Guid.NewGuid(),
                Message = request.Message

            };

            string messageBody = Newtonsoft.Json.JsonConvert.SerializeObject(messageToQueue);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            message.MessageId = messageToQueue.FormulaStatusId.ToString();

            //Send message to Service Bus Queue
            try
            {
                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }

            //Create File in Blob Storage
            var blobName = messageToQueue.FormulaStatusId.ToString();
            var blobContent = "Pending";

            CreateBlob(BlobContainerName, blobName, blobContent);

            var response = new SubmitFormulaResponse
            {
                FormulaStatusId = messageToQueue.FormulaStatusId
            };

            return CreatedAtRoute("GetFormulaStatus", new SubmitFormulaResponse { FormulaStatusId = response.FormulaStatusId }, response);
        }

        public string GetBlob(string containerName, string fileName)
        {
            // Setup the connection to the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("BlobStorageConnectionString"));
            // Connect to the blob storage
            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
            // Connect to the blob container
            CloudBlobContainer container = serviceClient.GetContainerReference($"{containerName}");
            // Connect to the blob file
            CloudBlockBlob blob = container.GetBlockBlobReference($"{fileName}");
            // Get the blob file as text
            string contents = blob.DownloadTextAsync().Result;

            return contents;
        }

        public void CreateBlob(string containerName, string fileName, string blobContent)
        {
            // Setup the connection to the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("BlobStorageConnectionString"));
            // Connect to the blob storage
            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
            // Connect to the blob container
            CloudBlobContainer container = serviceClient.GetContainerReference($"{containerName}");
            // Connect to the blob file
            CloudBlockBlob blob = container.GetBlockBlobReference($"{fileName}");
            //Upload blob
            blob.UploadTextAsync($"{blobContent}");
        }
    }

}
