using LMS_Project.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace LMS_Project.Application.Interfaces;

public interface IFileService
{
    /// <summary>
    /// ZIP dosyasını yükler ve belirtilen dizine çıkarır
    /// </summary>
    Task<Result<string>> UploadAndExtractZipAsync(
        IFormFile zipFile,
        Guid moduleId,
        Guid trainingId,
        Guid subTopicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Alt başlığın ZIP ve çıkarılmış dosyalarını siler
    /// </summary>
    Task<Result> DeleteSubTopicFilesAsync(
        Guid moduleId,
        Guid trainingId,
        Guid subTopicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Modülün tüm dosyalarını siler
    /// </summary>
    Task<Result> DeleteModuleFilesAsync(
        Guid moduleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Eğitimin tüm dosyalarını siler
    /// </summary>
    Task<Result> DeleteTrainingFilesAsync(
        Guid moduleId,
        Guid trainingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosya yükler (resim, video vb.)
    /// </summary>
    Task<Result<string>> UploadFileAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosya siler
    /// </summary>
    Task<Result> DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosyanın var olup olmadığını kontrol eder
    /// </summary>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Alt başlık HTML dosyasının yolunu döndürür
    /// </summary>
    string GetSubTopicHtmlPath(Guid moduleId, Guid trainingId, Guid subTopicId);

    /// <summary>
    /// Duyuru resmi yükler
    /// </summary>
    Task<Result<string>> UploadAnnouncementImageAsync(
        IFormFile imageFile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Duyuru resmini siler
    /// </summary>
    Task<Result> DeleteAnnouncementImageAsync(
        string imagePath,
        CancellationToken cancellationToken = default);
}
