namespace CatalogMicroservice.Services
{
    public interface IAmazonS3Service
    {
        Task<string> UploadFileAsync(IFormFile file);
    }

}
