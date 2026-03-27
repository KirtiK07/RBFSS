using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Services.Interfaces;

namespace RBFSS.Controllers
{
    [Authorize]
    public class FolderController : Controller
    {
        private readonly IFolderService _folderService;
        private readonly IFileService _fileService;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FolderController(IFolderService folderService, IFileService fileService,
            IAuditLogService auditLogService, UserManager<ApplicationUser> userManager)
        {
            _folderService = folderService;
            _fileService = fileService;
            _auditLogService = auditLogService;
            _userManager = userManager;
        }

        // Browse: all authenticated users can browse folders
        public async Task<IActionResult> Index(int? folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var subFolders = folderId.HasValue
                ? await _folderService.GetSubFoldersAsync(folderId.Value)
                : await _folderService.GetRootFoldersAsync();

            // Admins see all files; others see only accessible files
            IEnumerable<UploadedFile> files;
            if (userRoles.Contains("Admin") || userRoles.Contains("Manager"))
            {
                files = await _folderService.GetFilesInFolderAsync(folderId);
            }
            else
            {
                // Users: files in folder that are either theirs or shared with them
                var allFolderFiles = await _folderService.GetFilesInFolderAsync(folderId);
                var accessibleFiles = new List<UploadedFile>();
                foreach (var f in allFolderFiles)
                {
                    var canRead = await _fileService.CanUserAccessFileAsync(user!.Id, f.FileId, "read", userRoles);
                    if (canRead) accessibleFiles.Add(f);
                }
                files = accessibleFiles;
            }

            var breadcrumb = folderId.HasValue
                ? await _folderService.GetBreadcrumbAsync(folderId.Value)
                : Enumerable.Empty<Folder>();

            var currentFolder = folderId.HasValue
                ? await _folderService.GetFolderByIdAsync(folderId.Value)
                : null;

            var vm = new FolderBrowserViewModel
            {
                CurrentFolder = currentFolder,
                SubFolders = subFolders,
                Files = files,
                BreadcrumbPath = breadcrumb
            };

            return View(vm);
        }

        // GET: Create folder — Admin & Manager only
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Create(int? parentFolderId)
        {
            ViewBag.ParentFolderId = parentFolderId;
            ViewBag.ParentFolder = parentFolderId.HasValue
                ? await _folderService.GetFolderByIdAsync(parentFolderId.Value)
                : null;
            return View(new CreateFolderDto { ParentFolderId = parentFolderId });
        }

        // POST: Create folder — Admin & Manager only
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateFolderDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            var folder = await _folderService.CreateFolderAsync(model, user!.Id);

            await _auditLogService.LogActionAsync(user.Id, "CREATE_FOLDER",
                folder.FolderName, GetClientIpAddress(),
                $"Created folder: {folder.FolderName}");

            TempData["Success"] = $"Folder '{folder.FolderName}' created successfully.";
            return RedirectToAction("Index", new { folderId = model.ParentFolderId });
        }

        // POST: Delete folder — Admin & Manager only
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Delete(int folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var folder = await _folderService.GetFolderByIdAsync(folderId);
            var parentId = folder?.ParentFolderId;

            var result = await _folderService.DeleteFolderAsync(folderId, user!.Id, userRoles);
            if (result)
            {
                await _auditLogService.LogActionAsync(user.Id, "DELETE_FOLDER",
                    folder?.FolderName ?? $"FolderId:{folderId}", GetClientIpAddress());
                TempData["Success"] = "Folder deleted.";
            }
            else
            {
                TempData["Error"] = "Cannot delete this folder. You may not have permission.";
            }

            return RedirectToAction("Index", new { folderId = parentId });
        }

        private string GetClientIpAddress()
            => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    public class FolderBrowserViewModel
    {
        public Folder? CurrentFolder { get; set; }
        public IEnumerable<Folder> SubFolders { get; set; } = new List<Folder>();
        public IEnumerable<UploadedFile> Files { get; set; } = new List<UploadedFile>();
        public IEnumerable<Folder> BreadcrumbPath { get; set; } = new List<Folder>();
    }
}
