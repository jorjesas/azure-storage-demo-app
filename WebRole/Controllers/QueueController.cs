using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WebRole.Controllers
{
    public class QueueController : Controller
    {
        // GET: Queue
        public ActionResult Index()
        {
            return View();
        }

        public string GetMessage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=allegrov6backups;AccountKey=9ac06d2uDIPOwYr80dLsOb9e5EmQV4ioTPQXIYZf2zVp096W8Frq9ACrHTub8s0asLnTYGG+WruxPrPltGqEAQ==");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("mytable");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // read the latest messages from the table            
            TableOperation retrieveOperation = TableOperation.Retrieve<MyMessages>("Partition0", "Row0");

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            if (retrievedResult.Result != null)
            {
                return ((MyMessages)retrievedResult.Result).Messages;
            }
            else
                return "Failed to retrieve the messages";
        }

        public void ClearMessages()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=allegrov6backups;AccountKey=9ac06d2uDIPOwYr80dLsOb9e5EmQV4ioTPQXIYZf2zVp096W8Frq9ACrHTub8s0asLnTYGG+WruxPrPltGqEAQ==");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("mytable");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // read the latest messages from the table            
            TableOperation retrieveOperation = TableOperation.Retrieve<MyMessages>("Partition0", "Row0");

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            if (retrievedResult.Result != null)
            {
                TableOperation deleteOperation = TableOperation.Delete((MyMessages)retrievedResult.Result);

                table.Execute(deleteOperation);
            }
        }

        public void SendMessage(string inputMessage)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=allegrov6backups;AccountKey=9ac06d2uDIPOwYr80dLsOb9e5EmQV4ioTPQXIYZf2zVp096W8Frq9ACrHTub8s0asLnTYGG+WruxPrPltGqEAQ==");
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("myqueue");

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();

            // send a message to the queue
            CloudQueueMessage message = new CloudQueueMessage(string.Format("({0}) App says: {1}", DateTime.Now, inputMessage));
            queue.AddMessage(message);
        }
    }

    public class MyMessages : TableEntity
    {
        public DateTime LastUpdated { get; set; }
        public string Messages { get; set; }
    }

}