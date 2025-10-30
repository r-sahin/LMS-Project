using LMS_Project.Application.Interfaces;
using LMS_Project.Domain.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;

namespace LMS_Project.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 104857600; // 100 MB

    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<Result<string>> UploadAndExtractZipAsync(
        IFormFile zipFile,
        Guid moduleId,
        Guid trainingId,
        Guid subTopicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validasyonlar
            if (zipFile == null || zipFile.Length == 0)
            {
                return Result<string>.Failure("Dosya boş olamaz.");
            }

            if (zipFile.Length > MaxFileSize)
            {
                return Result<string>.Failure($"Dosya boyutu {MaxFileSize / 1024 / 1024} MB'dan büyük olamaz.");
            }

            if (!Path.GetExtension(zipFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return Result<string>.Failure("Sadece ZIP dosyaları yüklenebilir.");
            }

            // Klasör yapısını oluştur: wwwroot/content/modules/{moduleId}/trainings/{trainingId}/subtopics/{subTopicId}
            var contentPath = Path.Combine(
                _environment.WebRootPath,
                "content",
                "modules",
                moduleId.ToString(),
                "trainings",
                trainingId.ToString(),
                "subtopics",
                subTopicId.ToString()
            );

            // Eğer klasör varsa önce temizle
            if (Directory.Exists(contentPath))
            {
                Directory.Delete(contentPath, true);
            }

            Directory.CreateDirectory(contentPath);

            // ZIP dosyasını kaydet
            var zipPath = Path.Combine(contentPath, "content.zip");
            using (var stream = new FileStream(zipPath, FileMode.Create))
            {
                await zipFile.CopyToAsync(stream, cancellationToken);
            }

            // ZIP'i çıkar
            var extractPath = Path.Combine(contentPath, "files");
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);

            // index.html veya ilk HTML dosyasını bul
            var htmlFiles = Directory.GetFiles(extractPath, "*.html", SearchOption.AllDirectories);

            string htmlFilePath;
            if (htmlFiles.Any(f => Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase)))
            {
                htmlFilePath = htmlFiles.First(f => Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase));
            }
            else if (htmlFiles.Any())
            {
                htmlFilePath = htmlFiles.First();
            }
            else
            {
                return Result<string>.Failure("ZIP içinde HTML dosyası bulunamadı.");
            }

            // Relative path döndür
            var relativePath = htmlFilePath.Replace(_environment.WebRootPath, "").Replace("\\", "/");

            return Result<string>.Success(
                relativePath,
                "ZIP dosyası başarıyla yüklendi ve çıkarıldı.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Dosya yüklenirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteSubTopicFilesAsync(
        Guid moduleId,
        Guid trainingId,
        Guid subTopicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contentPath = Path.Combine(
                _environment.WebRootPath,
                "content",
                "modules",
                moduleId.ToString(),
                "trainings",
                trainingId.ToString(),
                "subtopics",
                subTopicId.ToString()
            );

            if (Directory.Exists(contentPath))
            {
                await Task.Run(() => Directory.Delete(contentPath, true), cancellationToken);
            }

            return Result.Success("Dosyalar başarıyla silindi.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Dosyalar silinirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteModuleFilesAsync(
        Guid moduleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var modulePath = Path.Combine(
                _environment.WebRootPath,
                "content",
                "modules",
                moduleId.ToString()
            );

            if (Directory.Exists(modulePath))
            {
                await Task.Run(() => Directory.Delete(modulePath, true), cancellationToken);
            }

            return Result.Success("Modül dosyaları başarıyla silindi.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Modül dosyaları silinirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteTrainingFilesAsync(
        Guid moduleId,
        Guid trainingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var trainingPath = Path.Combine(
                _environment.WebRootPath,
                "content",
                "modules",
                moduleId.ToString(),
                "trainings",
                trainingId.ToString()
            );

            if (Directory.Exists(trainingPath))
            {
                await Task.Run(() => Directory.Delete(trainingPath, true), cancellationToken);
            }

            return Result.Success("Eğitim dosyaları başarıyla silindi.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eğitim dosyaları silinirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<string>> UploadFileAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Result<string>.Failure("Dosya boş olamaz.");
            }

            if (file.Length > MaxFileSize)
            {
                return Result<string>.Failure($"Dosya boyutu {MaxFileSize / 1024 / 1024} MB'dan büyük olamaz.");
            }

            // Dosya adını güvenli hale getir
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            var safeFileName = $"{fileName}_{Guid.NewGuid()}{extension}";

            var uploadPath = Path.Combine(_environment.WebRootPath, folder);
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var relativePath = Path.Combine(folder, safeFileName).Replace("\\", "/");
            return Result<string>.Success($"/{relativePath}", "Dosya başarıyla yüklendi.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Dosya yüklenirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Result.Success("Dosya yolu belirtilmemiş.");
            }

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath), cancellationToken);
            }

            return Result.Success("Dosya başarıyla silindi.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Dosya silinirken hata oluştu: {ex.Message}");
        }
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.FromResult(false);
        }

        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        return Task.FromResult(File.Exists(fullPath));
    }

    public string GetSubTopicHtmlPath(Guid moduleId, Guid trainingId, Guid subTopicId)
    {
        return $"/content/modules/{moduleId}/trainings/{trainingId}/subtopics/{subTopicId}/files/index.html";
    }

    public async Task<Result<string>> UploadAnnouncementImageAsync(
        IFormFile imageFile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return Result<string>.Failure("Resim dosyası boş olamaz.");
            }

            if (imageFile.Length > 10 * 1024 * 1024) // 10 MB
            {
                return Result<string>.Failure("Resim dosyası en fazla 10 MB olabilir.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return Result<string>.Failure("Geçersiz resim formatı. İzin verilen: jpg, jpeg, png, gif, webp");
            }

            var fileName = $"announcement_{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(_environment.WebRootPath, "content", "announcements");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream, cancellationToken);
            }

            var relativePath = $"/content/announcements/{fileName}";
            return Result<string>.Success(relativePath, "Resim başarıyla yüklendi.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Resim yüklenirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAnnouncementImageAsync(
        string imagePath,
        CancellationToken cancellationToken = default)
    {
        return await DeleteFileAsync(imagePath, cancellationToken);
    }
}
