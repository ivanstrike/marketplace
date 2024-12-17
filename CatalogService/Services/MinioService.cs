using Minio;
using Minio.DataModel.Args;

namespace CatalogMicroservice.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;

        public MinioService(IConfiguration configuration)
        {
            _configuration = configuration;

            _minioClient = new MinioClient()
                .WithEndpoint(_configuration["Minio:Endpoint"])
                .WithCredentials(_configuration["Minio:AccessKey"], _configuration["Minio:SecretKey"])
                .Build();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var bucketName = _configuration["Minio:BucketName"];
            var objectName = $"uploads/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Убедимся, что бакет существует, или создадим его
            bool bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }

           
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length)
                .WithContentType(file.ContentType));

           
            return $"{_configuration["Minio:Endpoint"]}/{bucketName}/{objectName}";
        }

        public async Task<Stream?> DownloadFileAsync(string objectName)
        {
            var bucketName = _configuration["Minio:BucketName"];
            var memoryStream = new MemoryStream();

            try
            {
                
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream => stream.CopyToAsync(memoryStream)));

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string objectName)
        {
            var bucketName = _configuration["Minio:BucketName"];

            try
            {
                
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }
    }
}


