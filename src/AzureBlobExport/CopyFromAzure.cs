using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBlobExport
{
    public class CopyFromAzure
    {
        public async Task Copy(DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            var table = GetTable();
            var partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ConfigurationManager.AppSettings["TablePartionKey"]);
            var beginingOfDataCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, dateTimeFrom.ToString("yyyyMMddHHmmssfff"));
            var primaryCombination = TableQuery.CombineFilters(partitionKeyCondition, TableOperators.And, beginingOfDataCondition);
            var endOfDataCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, dateTimeTo.ToString("yyyyMMddHHmmssfff"));
            var tableCombination = TableQuery.CombineFilters(primaryCombination, TableOperators.And, endOfDataCondition);
            var tableOperation = new TableQuery<TableRecordEntity>().Where(tableCombination);

            Console.WriteLine($"Getting Blob entities from '{dateTimeFrom}' to '{dateTimeTo}'...");

            var queryResult = table.ExecuteQuery(tableOperation);
            var siriTableEntities = queryResult.ToList();

            const int batchCount = 512;
            var batches = siriTableEntities.Split(batchCount);

            Console.WriteLine($"Total item count: {siriTableEntities.Count} will split into {siriTableEntities.Count / batchCount} batches ({batchCount} each).");

            var dir = new DirectoryInfo(ConfigurationManager.AppSettings["SaveDirectory"]);
            if(!dir.Exists)
                dir.Create();

            var cloudBlobContainer = GetBlobContainer();

            var sw = new Stopwatch();
            sw.Start();
            
            foreach (var batch in batches)
            {
                var spin = new ConsoleSpinner();

                Console.WriteLine($"Downloading batch {batch.Item1}...");

                var sw2 = new Stopwatch();
                sw2.Start();

                await batch.Item2.ForEachAsync(entity => WriteBlobToDiskAsyncAsync(entity, cloudBlobContainer), (entity, result) => { spin.Turn(); });

                sw2.Stop();
                Console.WriteLine($"Took: {sw2.ElapsedMilliseconds}ms");
            }

            sw.Stop();
            Console.WriteLine($"Total: {sw.Elapsed}");
        }

        private async Task<string> WriteBlobToDiskAsyncAsync(TableRecordEntity entity, CloudBlobContainer cloudBlobContainer)
        {
            var blobReference = cloudBlobContainer.GetBlockBlobReference(entity.BlobReference);
            using (var memoryStream = new MemoryStream())
            {
                await blobReference.DownloadToStreamAsync(memoryStream);

                var stream = Encoding.UTF8.GetString(memoryStream.ToArray());
                var fileNameParsed = entity.BlobReference.Replace(' ', '_');
                fileNameParsed = fileNameParsed.Replace(':', '_');

                var path = Path.Combine(ConfigurationManager.AppSettings["SaveDirectory"], fileNameParsed + ".txt");
                var file = new FileInfo(path);

                File.WriteAllText(file.FullName, stream);

                return path;
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
