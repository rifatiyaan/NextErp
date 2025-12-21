using Microsoft.AspNetCore.Http;

namespace NextErp.Application.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task DeleteImageAsync(string publicId);
    }
}
