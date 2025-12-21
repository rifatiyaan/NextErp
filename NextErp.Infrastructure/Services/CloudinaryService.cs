using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NextErp.Application.Interfaces;
using System.Security.Principal;
using System.Linq;

namespace NextErp.Infrastructure.Services
{
    public class CloudinaryService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be empty");

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                EagerTransforms = new List<Transformation>
                {
                    new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
                }
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            // Return the eager transformed URL if available, otherwise the main URL
            var eagerTransform = uploadResult.Eager?.FirstOrDefault();
            if (eagerTransform != null)
            {
                return eagerTransform.SecureUrl.ToString();
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            
             if (result.Error != null)
            {
                throw new Exception(result.Error.Message);
            }
        }
    }
}
