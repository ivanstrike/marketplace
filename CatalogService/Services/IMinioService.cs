namespace CatalogMicroservice.Services
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(IFormFile file);
        public Task<Stream?> DownloadFileAsync(string objectName);
        public Task<bool> DeleteFileAsync(string objectName);

    }

}
