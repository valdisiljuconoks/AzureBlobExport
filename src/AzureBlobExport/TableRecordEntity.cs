using System;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBlobExport
{
    public class TableRecordEntity : TableEntity
    {
        public TableRecordEntity(string uniqueIdentifier, string partitionIdentifer)
        {
            PartitionKey = partitionIdentifer;
            RowKey = uniqueIdentifier;
        }

        public TableRecordEntity() { }

        public DateTime RecordDate { get; set; }

        public string BlobReference { get; set; }
    }
}
