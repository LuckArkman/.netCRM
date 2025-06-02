namespace Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<byte[]> DownloadFileAsync(string fileUrl);
        Task<bool> DeleteFileAsync(string fileUrl);
    }
}