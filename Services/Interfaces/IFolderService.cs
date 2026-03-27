using RBFSS.Models;
using RBFSS.Models.DTOs;

namespace RBFSS.Services.Interfaces
{
    public interface IFolderService
    {
        Task<IEnumerable<Folder>> GetRootFoldersAsync();
        Task<IEnumerable<Folder>> GetSubFoldersAsync(int parentFolderId);
        Task<IEnumerable<Folder>> GetAllFoldersAsync();
        Task<Folder?> GetFolderByIdAsync(int folderId);
        Task<IEnumerable<Folder>> GetBreadcrumbAsync(int folderId);
        Task<Folder?> CreateFolderAsync(CreateFolderDto dto, string createdById);
        Task<bool> DeleteFolderAsync(int folderId, string userId, IEnumerable<string> userRoles);
        Task<IEnumerable<UploadedFile>> GetFilesInFolderAsync(int? folderId);
    }
}
