using RBFSS.Models;
using RBFSS.Models.DTOs;

namespace RBFSS.Services.Interfaces
{
    public interface IFileService
    {
        Task<IEnumerable<UploadedFile>> GetAllFilesAsync();
        Task<IEnumerable<UploadedFile>> GetUserFilesAsync(string userId, IEnumerable<string> userRoles);
        Task<UploadedFile?> GetFileByIdAsync(int fileId);
        Task<UploadedFile?> UploadFileAsync(FileUploadDto fileUploadDto, string userId, string uploadsPath);
        Task<bool> DeleteFileAsync(int fileId, string userId, IEnumerable<string> userRoles);
        Task<bool> CanUserAccessFileAsync(string userId, int fileId, string action, IEnumerable<string> userRoles);
        Task<bool> ShareFileWithUsersAsync(int fileId, List<string> userIds, string sharedByUserId);
        Task<bool> EditFileAsync(int fileId, string newFileName, string description, string userId, IEnumerable<string> userRoles);
        Task<IEnumerable<RBFSS.Models.Folder>> GetAllFoldersForSelectAsync();
        Task<Dictionary<string, object>> GetFileStatisticsAsync();
        Task<string?> GetFilePhysicalPathAsync(int fileId);
        bool IsValidFileType(string fileName, string[] allowedExtensions);
        bool IsValidFileSize(long fileSize, long maxFileSize);
    }
}