using Microsoft.EntityFrameworkCore;
using RBFSS.Data;
using RBFSS.Models;
using RBFSS.Repositories.Interfaces;

namespace RBFSS.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly ApplicationDbContext _context;

        public FolderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Folder>> GetRootFoldersAsync()
        {
            return await _context.Folders
                .Include(f => f.CreatedBy)
                .Include(f => f.SubFolders)
                .Where(f => f.ParentFolderId == null && !f.IsDeleted)
                .OrderBy(f => f.FolderName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Folder>> GetSubFoldersAsync(int parentFolderId)
        {
            return await _context.Folders
                .Include(f => f.CreatedBy)
                .Include(f => f.SubFolders)
                .Where(f => f.ParentFolderId == parentFolderId && !f.IsDeleted)
                .OrderBy(f => f.FolderName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
        {
            return await _context.Folders
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.FolderName)
                .ToListAsync();
        }

        public async Task<Folder?> GetFolderByIdAsync(int folderId)
        {
            return await _context.Folders
                .Include(f => f.CreatedBy)
                .Include(f => f.ParentFolder)
                .Include(f => f.SubFolders.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(f => f.FolderId == folderId && !f.IsDeleted);
        }

        public async Task<IEnumerable<Folder>> GetBreadcrumbAsync(int folderId)
        {
            var breadcrumb = new List<Folder>();
            var current = await _context.Folders
                .Include(f => f.ParentFolder)
                .FirstOrDefaultAsync(f => f.FolderId == folderId);

            while (current != null)
            {
                breadcrumb.Insert(0, current);
                current = current.ParentFolderId.HasValue
                    ? await _context.Folders
                        .Include(f => f.ParentFolder)
                        .FirstOrDefaultAsync(f => f.FolderId == current.ParentFolderId)
                    : null;
            }

            return breadcrumb;
        }

        public async Task<Folder> CreateFolderAsync(Folder folder)
        {
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
            return folder;
        }

        public async Task<bool> DeleteFolderAsync(int folderId)
        {
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder == null) return false;
            folder.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UploadedFile>> GetFilesInFolderAsync(int? folderId)
        {
            return await _context.UploadedFiles
                .Include(f => f.UploadedBy)
                .Where(f => f.FolderId == folderId && !f.IsDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }
    }
}
