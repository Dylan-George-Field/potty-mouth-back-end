using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using TryScanMe.Functions.Entities;

namespace TryScanMe.Functions.Repository
{
    internal static class BlobStorage
    {
        private static CloudBlobClient _client;

        static BlobStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("storageAccount"));
            _client = storageAccount.CreateCloudBlobClient();
        }

        public static CloudBlobContainer GetContainer(string containerName)
        {  
            var container = _client.GetContainerReference(containerName);

            container.CreateIfNotExists();

            return container;
        }

        public static CloudBlockBlob GetBlob(string containerName, string blobName)
        {
            var container = GetContainer(containerName);

            return container.GetBlockBlobReference(blobName);
        }

        public static bool BlobExists(string containerName, string blobName)
        {
            var blob = GetBlob(containerName, blobName);

            return blob.Exists();
        }

        public static string GetBlobText(string containerName, string blobName)
        {
            var blob = GetBlob(containerName, blobName);
            string text;

            using (var memoryStream = new MemoryStream())
            {
                blob.DownloadToStream(memoryStream);
                text = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            return text;
        }

        //This should be somewhere else 
        public static WallEntity GetWall(string wallId)
        {
            var path = String.Format($"{wallId}/wall");

            if (!BlobExists(Constants.BlobContainerNames.Wall, path)) return null;

            var text = GetBlobText(Constants.BlobContainerNames.Wall, path);

            var wall = JsonConvert.DeserializeObject<WallEntity>(text);

            return wall;
        }
    }
}
