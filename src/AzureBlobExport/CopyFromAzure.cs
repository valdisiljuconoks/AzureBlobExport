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
            var partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "siriFeed");
            var beginingOfDataCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, dateTimeFrom.ToString("yyyyMMddHHmmssfff"));
            var primaryCombination = TableQuery.CombineFilters(partitionKeyCondition, TableOperators.And, beginingOfDataCondition);
            var tableCombination = primaryCombination;

            //used to threshold how much data we retrieve from azure
            var thirdCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, dateTimeTo.ToString("yyyyMMddHHmmssfff"));
            tableCombination = TableQuery.CombineFilters(tableCombination, TableOperators.And, thirdCondition);

            var tableOperation = new TableQuery<SiriTableEntity>().Where(tableCombination);
            var queryResult = table.ExecuteQuery(tableOperation);
            var siriTableEntity = queryResult.ToList();

            foreach (var entity in siriTableEntity)
            {
                var vehicleBlob = GetSiriCloudBlobContainer().GetBlockBlobReference(entity.BlobReference);
                using (var memoryStream = new MemoryStream())
                {
                    vehicleBlob.DownloadToStream(memoryStream);
                    var stream = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var fileNameParsed = entity.BlobReference.Replace(' ', '_');
                    fileNameParsed = fileNameParsed.Replace(':', '_');
                    var fileName = @"C:\BLOBS\" + fileNameParsed + ".txt";
                    var file = new FileInfo(fileName);
                    file.Directory?.Create(); // If the directory already exists, this method does nothing.
                    File.WriteAllText(file.FullName, stream);
                }
            }
        }

        private CloudTable GetTable()
        {
            var table = GetTableClient("StorageTimeMachineConnectionString").GetTableReference("SiriVehicleMonitoringRecords");
            table.CreateIfNotExists();
            return table;
        }

        private static CloudTableClient GetTableClient(string connectionName = "StorageConnectionString")
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            return storageAccount.CreateCloudTableClient();
        }

        public CloudBlobContainer GetSiriCloudBlobContainer()
        {
            var client = GetBlobClient("StorageTimeMachineConnectionString");
            var container = client.GetContainerReference("siridatafeed");
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            return container;
        }

        private static CloudBlobClient GetBlobClient(string connectionName = "StorageConnectionString")
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            return storageAccount.CreateCloudBlobClient();
        }
    }
}
