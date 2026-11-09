using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }

    public class BlobService : IBlobService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _connectionString;

        public BlobService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _connectionString = configuration.GetConnectionString("AzureBlobStorage") ?? "";
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File is empty");

            // If we have an Azure Connection String, use real Azure Blob Storage
            if (!string.IsNullOrEmpty(_connectionString))
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("event-images");
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var blobClient = containerClient.GetBlobClient(fileName);
                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                return blobClient.Uri.ToString();
            }
            else
            {
                // FALLBACK: Save locally to simulate Azure Blob Storage
                // Safely get the web root path or default to current directory
                string rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(rootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return $"/uploads/{fileName}";
            }
        }
    }
}