using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace TryScanMe.Functions
{
    internal static class TableStorage
    {
        private static CloudTableClient _client;

        static TableStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("storageAccount"));
            _client = storageAccount.CreateCloudTableClient();
        }

        public static CloudTable GetTable(string tableName)
        {
            var table = _client.GetTableReference(tableName);
            table.CreateIfNotExists();

            return table;
        }

        public static IEnumerable<TEntity> QueryByPartitionKey<TEntity>(string tableName, string partitionKey) where TEntity : TableEntity, new()
        {
            var table = GetTable(tableName);

            return table.ExecuteQuery(new TableQuery<TEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)));
        }

        public static void Insert<T>(string tableName, T entity) where T : ITableEntity
        {
            var table = GetTable(tableName);

            var insert = TableOperation.InsertOrReplace(entity);

            table.Execute(insert);
        }

        public static TEntity GetEntity<TEntity>(string tableName, string partitionKey, string rowKey) where TEntity : TableEntity
        {
            var table = GetTable(tableName);

            var retrieveOperation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

            var entity = table.Execute(retrieveOperation).Result as TEntity;

            return entity;
        }
    }
}
