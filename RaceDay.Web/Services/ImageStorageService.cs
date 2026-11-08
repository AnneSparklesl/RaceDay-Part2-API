namespace RaceDay.Web.Services;

// Saves uploaded images into wwwroot/uploads so the site can show them
public class ImageStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxBytes = 2 * 1024 * 1024;

    private readonly IWebHostEnvironment _env;

    public ImageStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    // Returns the web path (e.g. /uploads/banners/abc.jpg) or a friendly error message
    public async Task<(string? Url, string? Error)> SaveAsync(IFormFile? file, string folder)
    {
        if (file == null || file.Length == 0)
            return (null, "Please choose an image to upload.");

        if (file.Length > MaxBytes)
            return (null, "That image is too large. Please choose one under 2 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/"))
            return (null, "Please upload a JPG, PNG, GIF or WEBP image.");

        var directory = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        return ($"/uploads/{folder}/{fileName}", null);
    }

    // Removes an old image (only ever inside wwwroot/uploads)
    public void Delete(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/"))
            return;

        var relative = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_env.WebRootPath, relative));
        var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));

        if (fullPath.StartsWith(uploadsRoot) && File.Exists(fullPath))
            File.Delete(fullPath);
    }
}