using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace TryScanMe.Functions.Entities
{
    public class TemporaryUrl : TableEntity
    {
        public TemporaryUrl(string guid)
        {
            RealUrl = guid;

            var tempGuid = Guid.NewGuid().ToString("N");
            PartitionKey = tempGuid;
            RowKey = tempGuid;
        }

        public TemporaryUrl() { }

        public string RealUrl { get; set; }

        public DateTime Expiry { get; set; } = DateTime.UtcNow.AddMinutes(3);
    }
}
