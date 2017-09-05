using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBlobExport
{
    public class CopyFromAzure
    {
        private readonly string _accountName;
        private readonly string _accessKey;
        private readonly string _blobContainerName;
        private readonly string _sasToken;
        private readonly string _saveDirectory;
        private readonly string _tableName;
        private readonly string _tablePartitionKey;

        public CopyFromAzure(string accountName, string accessKey, string sasToken, string tableName, string blobContainerName, string saveDirectory, string tablePartitionKey)
        {
            if(!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(sasToken))
                throw new ArgumentException("Account Access Key and SAS Token cannot be used together");
            
            _accountName = accountName;
            _accessKey = accessKey;
            _sasToken = sasToken;
            _tableName = tableName;
            _blobContainerName = blobContainerName;
            _saveDirectory = saveDirectory;
            _tablePartitionKey = tablePartitionKey;
        }

        public async Task Copy(DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            Console.WriteLine($"Getting Blob entities from '{dateTimeFrom}' to '{dateTimeTo}'...");

            var table = GetTable();
            var partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _tablePartitionKey);
            var beginingOfDataCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, dateTimeFrom.ToString("yyyyMMddHHmmssfff"));
            var primaryCombination = TableQuery.CombineFilters(partitionKeyCondition, TableOperators.And, beginingOfDataCondition);
            var endOfDataCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, dateTimeTo.ToString("yyyyMMddHHmmssfff"));
            var tableCombination = TableQuery.CombineFilters(primaryCombination, TableOperators.And, endOfDataCondition);
            var tableOperation = new TableQuery<TableRecordEntity>().Where(tableCombination);

            var queryResult = table.ExecuteQuery(tableOperation);
            var siriTableEntities = queryResult.ToList();

            const int batchCount = 512;
            var batches = siriTableEntities.Split(batchCount);

            Console.WriteLine($"Total item count: {siriTableEntities.Count} will split into {siriTableEntities.Count / batchCount} batches ({batchCount} each).");

            var dir = new DirectoryInfo(_saveDirectory);
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

                var path = Path.Combine(_saveDirectory, fileNameParsed + ".txt");
                var file = new FileInfo(path);

                File.WriteAllText(file.FullName, stream);

                return path;
            }
        }

        private CloudTable GetTable()
        {
            var credentials = new StorageCredentials(_accountName, _accessKey);
            
            if(!string.IsNullOrEmpty(_sasToken))
                credentials = new StorageCredentials(_sasToken);
                
            var tableClient = new CloudTableClient(new Uri($@"https://{_accountName}.table.core.windows.net/"), credentials);

            var table = tableClient.GetTableReference(_tableName);
            return table;
        }

        public CloudBlobContainer GetBlobContainer()
        {
            return new CloudBlobContainer(new Uri($@"https://{_accountName}.blob.core.windows.net/{_blobContainerName}?{_sasToken}"));
        }
    }
}
