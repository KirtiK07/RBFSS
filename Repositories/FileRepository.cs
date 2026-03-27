using Microsoft.EntityFrameworkCore;
using RBFSS.Data;
using RBFSS.Models;
using RBFSS.Repositories.Interfaces;

namespace RBFSS.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly ApplicationDbContext _context;

        public FileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UploadedFile>> GetAllFilesAsync()
        {
            return await _context.UploadedFiles
                .Include(f => f.UploadedBy)
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UploadedFile>> GetFilesByUserIdAsync(string userId)
        {
            return await _context.UploadedFiles
                .Include(f => f.UploadedBy)
                .Where(f => f.UploadedById == userId && !f.IsDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UploadedFile>> GetFilesAccessibleToUserAsync(string userId, IEnumerable<string> userRoles)
        {
            var query = _context.UploadedFiles
                .Include(f => f.UploadedBy)
                .Include(f => f.FilePermissions)
                .Where(f => !f.IsDeleted);

            // Admin can see all files
            if (userRoles.Contains("Admin"))
            {
                return await query.OrderByDescending(f => f.UploadedAt).ToListAsync();
            }

            // Manager can see all shared files and their own files
            if (userRoles.Contains("Manager"))
            {
                query = query.Where(f =>
                    f.UploadedById == userId ||
                    f.FilePermissions.Any(fp => fp.UserId == userId && fp.CanRead));
            }
            else
            {
                // Regular users can only see files explicitly shared with them or their own files
                query = query.Where(f =>
                    f.UploadedById == userId ||
                    f.FilePermissions.Any(fp => fp.UserId == userId && fp.CanRead));
            }

            return await query.OrderByDescending(f => f.UploadedAt).ToListAsync();
        }

        public async Task<UploadedFile?> GetFileByIdAsync(int fileId)
        {
            return await _context.UploadedFiles
                .Include(f => f.UploadedBy)
                .Include(f => f.FilePermissions)
                .FirstOrDefaultAsync(f => f.FileId == fileId && !f.IsDeleted);
        }

        public async Task<UploadedFile> CreateFileAsync(UploadedFile file)
        {
            _context.UploadedFiles.Add(file);
            await _context.SaveChangesAsync();
            return file;
        }

        public async Task<bool> UpdateFileAsync(UploadedFile file)
        {
            try
            {
                file.LastModified = DateTime.UtcNow;
                _context.UploadedFiles.Update(file);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _context.UploadedFiles.FindAsync(fileId);
            if (file != null)
            {
                file.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> CreateFilePermissionAsync(FilePermission permission)
        {
            try
            {
                // Check if a permission already exists for this file+user combo
                var existing = await _context.FilePermissions
                    .FirstOrDefaultAsync(fp => fp.FileId == permission.FileId && fp.UserId == permission.UserId);

                if (existing != null)
                {
                    // Update existing permission
                    existing.CanRead = permission.CanRead;
                    existing.CanWrite = permission.CanWrite;
                    existing.CanDelete = permission.CanDelete;
                    existing.Action = permission.Action;
                }
                else
                {
                    _context.FilePermissions.Add(permission);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CanUserAccessFileAsync(string userId, int fileId, string action)
        {
            var file = await _context.UploadedFiles
                .Include(f => f.FilePermissions)
                .FirstOrDefaultAsync(f => f.FileId == fileId && !f.IsDeleted);

            if (file == null) return false;

            // Owner can do anything with their files
            if (file.UploadedById == userId) return true;

            // Check explicit permissions
            var permission = file.FilePermissions.FirstOrDefault(fp => fp.UserId == userId);
            if (permission != null)
            {
                return action.ToLower() switch
                {
                    "read" => permission.CanRead,
                    "write" => permission.CanWrite,
                    "delete" => permission.CanDelete,
                    _ => false
                };
            }

            return false;
        }

        public async Task<int> GetTotalFilesCountAsync()
        {
            return await _context.UploadedFiles.CountAsync(f => !f.IsDeleted);
        }

        public async Task<long> GetTotalFileSizeAsync()
        {
            return await _context.UploadedFiles
                .Where(f => !f.IsDeleted)
                .SumAsync(f => f.FileSize);
        }

        public async Task<IEnumerable<UploadedFile>> GetRecentFilesAsync(int count = 10)
        {
            return await _context.UploadedFiles
                .Include(f => f.UploadedBy)
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}