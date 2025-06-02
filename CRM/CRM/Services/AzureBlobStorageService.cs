using Azure.Storage.Blobs;
using Interfaces;

namespace GabineteDigital.Infrastructure.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage");
            _containerName = configuration["AzureBlobStorage:ContainerName"];
        }

        private BlobContainerClient GetContainerClient()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            return blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var containerClient = GetContainerClient();
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

        public async Task<byte[]> DownloadFileAsync(string fileUrl)
        {
            var blobClient = new BlobClient(new Uri(fileUrl));
            var response = await blobClient.DownloadContentAsync();
            return response.Value.Content.ToArray();
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            var blobClient = new BlobClient(new Uri(fileUrl));
            return await blobClient.DeleteIfExistsAsync();
        }
    }
}