using RBFSS.Models;

namespace RBFSS.Repositories.Interfaces
{
    public interface IFolderRepository
    {
        Task<IEnumerable<Folder>> GetRootFoldersAsync();
        Task<IEnumerable<Folder>> GetSubFoldersAsync(int parentFolderId);
        Task<IEnumerable<Folder>> GetAllFoldersAsync();
        Task<Folder?> GetFolderByIdAsync(int folderId);
        Task<IEnumerable<Folder>> GetBreadcrumbAsync(int folderId);
        Task<Folder> CreateFolderAsync(Folder folder);
        Task<bool> DeleteFolderAsync(int folderId);
        Task<IEnumerable<UploadedFile>> GetFilesInFolderAsync(int? folderId);
    }
}
