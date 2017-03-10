using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBlobExport
{
    public class SiriTableEntity : TableEntity
    {
        public SiriTableEntity(string uniqueIdentifier, string partitionIdentifer)
        {
            PartitionKey = partitionIdentifer;
            RowKey = uniqueIdentifier;
        }

        public SiriTableEntity() { }

        public DateTime RecordDate { get; set; }

        public string BlobReference { get; set; }
    }
}
