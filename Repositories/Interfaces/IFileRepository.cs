using RBFSS.Models;

namespace RBFSS.Repositories.Interfaces
{
    public interface IFileRepository
    {
        Task<IEnumerable<UploadedFile>> GetAllFilesAsync();
        Task<IEnumerable<UploadedFile>> GetFilesByUserIdAsync(string userId);
        Task<IEnumerable<UploadedFile>> GetFilesAccessibleToUserAsync(string userId, IEnumerable<string> userRoles);
        Task<UploadedFile?> GetFileByIdAsync(int fileId);
        Task<UploadedFile> CreateFileAsync(UploadedFile file);
        Task<bool> UpdateFileAsync(UploadedFile file);
        Task<bool> DeleteFileAsync(int fileId);
        Task<bool> CanUserAccessFileAsync(string userId, int fileId, string action);
        Task<bool> CreateFilePermissionAsync(FilePermission permission);
        Task<int> GetTotalFilesCountAsync();
        Task<long> GetTotalFileSizeAsync();
        Task<IEnumerable<UploadedFile>> GetRecentFilesAsync(int count = 10);
    }
}