namespace CanPany.Application.Interfaces.Services;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "CanPany");
    Task<bool> DeleteImageAsync(string publicId);
}


