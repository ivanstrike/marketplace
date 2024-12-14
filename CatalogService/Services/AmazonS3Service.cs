using Amazon.S3.Model;
using Amazon.S3;

namespace CatalogMicroservice.Services
{
    public class AmazonS3Service : IAmazonS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public AmazonS3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var bucketName = _configuration["S3:BucketName"];
            var filePath = $"uploads/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = filePath,
                InputStream = memoryStream,
                ContentType = file.ContentType,
                AutoCloseStream = true
            };

            var response = await _s3Client.PutObjectAsync(putRequest);

            return $"https://{bucketName}.s3.amazonaws.com/{filePath}";
        }
    }
}
