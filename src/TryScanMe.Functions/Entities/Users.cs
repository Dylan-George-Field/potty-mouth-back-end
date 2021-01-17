using Microsoft.WindowsAzure.Storage.Table;

namespace TryScanMe.Functions.Entities
{
    public class Users : TableEntity
    {
        public string Email { get; set; }

        public Users(string nameId, string name)
        {
            PartitionKey = nameId;
            RowKey = name;
        }
    }
}
