using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBlobExport
{
    public class CopyFromAzure
    {
        public void Copy(DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            var table = GetTable();
            var partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ConfigurationManager.AppSettings["TablePartionKey"]);
            var beginingOfDataCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, dateTimeFrom.ToString("yyyyMMddHHmmssfff"));
            var primaryCombination = TableQuery.CombineFilters(partitionKeyCondition, TableOperators.And, beginingOfDataCondition);
            var endOfDataCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, dateTimeTo.ToString("yyyyMMddHHmmssfff"));
            var tableCombination = TableQuery.CombineFilters(primaryCombination, TableOperators.And, endOfDataCondition);
            var tableOperation = new TableQuery<SiriTableEntity>().Where(tableCombination);

            Console.WriteLine($"Getting Blob entities from '{dateTimeFrom}' to '{dateTimeTo}'...");

            var queryResult = table.ExecuteQuery(tableOperation);
            var siriTableEntities = queryResult.ToList();

            var batches = siriTableEntities.Split(500);

            Console.WriteLine($"Total item count: {siriTableEntities.Count}");

            var dir = new DirectoryInfo(ConfigurationManager.AppSettings["SaveDirectory"]);
            if(!dir.Exists)
                dir.Create();

            var batchIndex = 0;
            foreach (var batch in batches)
            {
                var startIndex = batchIndex * 500;
                var endIndex = batchIndex * 500 + 500;

                Console.WriteLine($"Downloading from {startIndex} to {(endIndex < siriTableEntities.Count ? endIndex : siriTableEntities.Count)}...");
                foreach (var entity in batch)
                {
                    var vehicleBlob = GetBlobContainer().GetBlockBlobReference(entity.BlobReference);
                    using (var memoryStream = new MemoryStream())
                    {
                        vehicleBlob.DownloadToStream(memoryStream);
                        var stream = Encoding.UTF8.GetString(memoryStream.ToArray());
                        var fileNameParsed = entity.BlobReference.Replace(' ', '_');
                        fileNameParsed = fileNameParsed.Replace(':', '_');

                        var path = Path.Combine(ConfigurationManager.AppSettings["SaveDirectory"], fileNameParsed + ".txt");
                        var file = new FileInfo(path);

                        File.WriteAllText(file.FullName, stream);
                    }
                }

                batchIndex++;
            }
        }

        private CloudTable GetTable()
        {
            var storageAccount = GetCloudStorageAccount();
            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference(ConfigurationManager.AppSettings["TableName"]);
            table.CreateIfNotExists();

            return table;
        }

        public CloudBlobContainer GetBlobContainer()
        {
            var storageAccount = GetCloudStorageAccount();
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("siridatafeed");

            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            return container;
        }

        private static CloudStorageAccount GetCloudStorageAccount()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TableStorageConnectionString"].ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            return storageAccount;
        }
    }
}
