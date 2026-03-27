using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Repositories.Interfaces;
using RBFSS.Services.Interfaces;

namespace RBFSS.Services
{
    public class FolderService : IFolderService
    {
        private readonly IFolderRepository _folderRepository;

        public FolderService(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        public async Task<IEnumerable<Folder>> GetRootFoldersAsync()
            => await _folderRepository.GetRootFoldersAsync();

        public async Task<IEnumerable<Folder>> GetSubFoldersAsync(int parentFolderId)
            => await _folderRepository.GetSubFoldersAsync(parentFolderId);

        public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
            => await _folderRepository.GetAllFoldersAsync();

        public async Task<Folder?> GetFolderByIdAsync(int folderId)
            => await _folderRepository.GetFolderByIdAsync(folderId);

        public async Task<IEnumerable<Folder>> GetBreadcrumbAsync(int folderId)
            => await _folderRepository.GetBreadcrumbAsync(folderId);

        public async Task<Folder?> CreateFolderAsync(CreateFolderDto dto, string createdById)
        {
            var folder = new Folder
            {
                FolderName = dto.FolderName.Trim(),
                ParentFolderId = dto.ParentFolderId,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow
            };
            return await _folderRepository.CreateFolderAsync(folder);
        }

        public async Task<bool> DeleteFolderAsync(int folderId, string userId, IEnumerable<string> userRoles)
        {
            var folder = await _folderRepository.GetFolderByIdAsync(folderId);
            if (folder == null) return false;

            // Only Admin can delete any folder; Manager can delete their own
            if (!userRoles.Contains("Admin") && folder.CreatedById != userId)
                return false;

            return await _folderRepository.DeleteFolderAsync(folderId);
        }

        public async Task<IEnumerable<UploadedFile>> GetFilesInFolderAsync(int? folderId)
            => await _folderRepository.GetFilesInFolderAsync(folderId);
    }
}
