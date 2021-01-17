using Microsoft.WindowsAzure.Storage.Table;

namespace TryScanMe.Functions.Entities
{
    public class Tracked : TableEntity
    {
        public Tracked(string nameId, string wallId)
        {
            PartitionKey = nameId;
            RowKey = wallId;
        }

        public Tracked() {}
    }
}
