using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Repositories.Interfaces;
using RBFSS.Services.Interfaces;

namespace RBFSS.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IConfiguration _configuration;

        public FileService(IFileRepository fileRepository, IFolderRepository folderRepository, IConfiguration configuration)
        {
            _fileRepository = fileRepository;
            _folderRepository = folderRepository;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UploadedFile>> GetAllFilesAsync()
        {
            return await _fileRepository.GetAllFilesAsync();
        }

        public async Task<IEnumerable<UploadedFile>> GetUserFilesAsync(string userId, IEnumerable<string> userRoles)
        {
            return await _fileRepository.GetFilesAccessibleToUserAsync(userId, userRoles);
        }

        public async Task<UploadedFile?> GetFileByIdAsync(int fileId)
        {
            return await _fileRepository.GetFileByIdAsync(fileId);
        }

        public async Task<UploadedFile?> UploadFileAsync(FileUploadDto fileUploadDto, string userId, string uploadsPath)
        {
            if (fileUploadDto.File == null || fileUploadDto.File.Length == 0)
                return null;

            // Validate file
            var allowedExtensions = _configuration.GetSection("FileSettings:AllowedExtensions").Get<string[]>() ?? new string[0];
            var maxFileSize = _configuration.GetValue<long>("FileSettings:MaxFileSize", 10485760); // 10MB default

            if (!IsValidFileType(fileUploadDto.File.FileName, allowedExtensions))
                return null;

            if (!IsValidFileSize(fileUploadDto.File.Length, maxFileSize))
                return null;

            // Ensure upload directory exists
            Directory.CreateDirectory(uploadsPath);

            // Generate unique file name
            var fileName = Path.GetFileNameWithoutExtension(fileUploadDto.File.FileName);
            var extension = Path.GetExtension(fileUploadDto.File.FileName);
            var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileUploadDto.File.CopyToAsync(stream);
            }

            // Create file record
            var uploadedFile = new UploadedFile
            {
                FileName = fileUploadDto.File.FileName,
                FilePath = filePath,
                FileType = fileUploadDto.File.ContentType,
                FileSize = fileUploadDto.File.Length,
                UploadedById = userId,
                UploadedAt = DateTime.UtcNow,
                Description = fileUploadDto.Description,
                FolderId = fileUploadDto.FolderId
            };

            var createdFile = await _fileRepository.CreateFileAsync(uploadedFile);

            // Handle sharing if specified
            if (fileUploadDto.ShareWithAllUsers || fileUploadDto.SharedWithUserIds.Any())
            {
                await ShareFileWithUsersAsync(createdFile.FileId, fileUploadDto.SharedWithUserIds, userId);
            }

            return createdFile;
        }

        public async Task<bool> DeleteFileAsync(int fileId, string userId, IEnumerable<string> userRoles)
        {
            var file = await _fileRepository.GetFileByIdAsync(fileId);
            if (file == null) return false;

            // Only Admin can delete any file; Manager and User can only delete their own
            if (!userRoles.Contains("Admin") && file.UploadedById != userId)
                return false;

            // Delete physical file
            if (File.Exists(file.FilePath))
            {
                File.Delete(file.FilePath);
            }

            // Soft delete from database
            return await _fileRepository.DeleteFileAsync(fileId);
        }

        public async Task<bool> CanUserAccessFileAsync(string userId, int fileId, string action, IEnumerable<string> userRoles)
        {
            // Admin can do anything
            if (userRoles.Contains("Admin"))
                return true;

            var file = await _fileRepository.GetFileByIdAsync(fileId);
            if (file == null) return false;

            var isOwner = file.UploadedById == userId;

            if (userRoles.Contains("Manager"))
            {
                // Manager: read/write/download any file, but edit/delete only own
                if (action.ToLower() == "delete" || action.ToLower() == "edit")
                    return isOwner;
                return true;
            }

            // User: edit/delete only own files
            if (action.ToLower() == "edit" || action.ToLower() == "delete")
                return isOwner;

            // User: read own or explicitly shared files
            if (action.ToLower() == "read")
            {
                if (isOwner) return true;
                return await _fileRepository.CanUserAccessFileAsync(userId, fileId, action);
            }

            return false;
        }

        public async Task<bool> EditFileAsync(int fileId, string newFileName, string description, string userId, IEnumerable<string> userRoles)
        {
            var file = await _fileRepository.GetFileByIdAsync(fileId);
            if (file == null) return false;

            // Only Admin can edit any file; others only their own
            if (!userRoles.Contains("Admin") && file.UploadedById != userId)
                return false;

            file.FileName = newFileName.Trim();
            file.Description = description;
            file.LastModified = DateTime.UtcNow;

            return await _fileRepository.UpdateFileAsync(file);
        }

        public async Task<bool> ShareFileWithUsersAsync(int fileId, List<string> userIds, string sharedByUserId)
        {
            var file = await _fileRepository.GetFileByIdAsync(fileId);
            if (file == null) return false;

            var success = true;
            foreach (var userId in userIds)
            {
                // Don't create a permission for the file owner — they already have full access
                if (userId == file.UploadedById) continue;

                var permission = new RBFSS.Models.FilePermission
                {
                    FileId = fileId,
                    UserId = userId,
                    Action = "READ",
                    Resource = file.FileName,
                    CanRead = true,
                    CanWrite = false,
                    CanDelete = false,
                    GrantedAt = DateTime.UtcNow
                };

                var result = await _fileRepository.CreateFilePermissionAsync(permission);
                if (!result) success = false;
            }

            return success;
        }

        public async Task<Dictionary<string, object>> GetFileStatisticsAsync()
        {
            var totalFiles = await _fileRepository.GetTotalFilesCountAsync();
            var totalSize = await _fileRepository.GetTotalFileSizeAsync();
            var recentFiles = await _fileRepository.GetRecentFilesAsync(10);

            return new Dictionary<string, object>
            {
                { "TotalFiles", totalFiles },
                { "TotalSizeBytes", totalSize },
                { "TotalSizeMB", Math.Round(totalSize / (1024.0 * 1024.0), 2) },
                { "RecentFiles", recentFiles }
            };
        }

        public async Task<string?> GetFilePhysicalPathAsync(int fileId)
        {
            var file = await _fileRepository.GetFileByIdAsync(fileId);
            return file?.FilePath;
        }

        public bool IsValidFileType(string fileName, string[] allowedExtensions)
        {
            if (allowedExtensions == null || allowedExtensions.Length == 0)
                return true; // If no restrictions, allow all

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Any(ext => ext.ToLowerInvariant() == extension);
        }

        public bool IsValidFileSize(long fileSize, long maxFileSize)
        {
            return fileSize > 0 && fileSize <= maxFileSize;
        }

        public async Task<IEnumerable<Folder>> GetAllFoldersForSelectAsync()
        {
            return await _folderRepository.GetAllFoldersAsync();
        }
    }
}