using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Services.Interfaces;

namespace RBFSS.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IFileService _fileService;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserController(IFileService fileService, IAuditLogService auditLogService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _fileService = fileService;
            _auditLogService = auditLogService;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var files = await _fileService.GetUserFilesAsync(user!.Id, userRoles);

            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            ViewBag.Folders = await _fileService.GetAllFoldersForSelectAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUploadDto model)
        {
            if (ModelState.IsValid && model.File != null)
            {
                var user = await _userManager.GetUserAsync(User);
                var uploadsPath = _configuration.GetValue<string>("FileSettings:UploadPath", "wwwroot/uploads") ?? "wwwroot/uploads";
                var fullUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsPath, "user");

                var uploadedFile = await _fileService.UploadFileAsync(model, user!.Id, fullUploadsPath);

                if (uploadedFile != null)
                {
                    await _auditLogService.LogActionAsync(user.Id, "UPLOAD_FILE", uploadedFile.FileName, GetClientIpAddress(),
                        $"Uploaded file: {uploadedFile.FileName}", uploadedFile.FileId);

                    TempData["Success"] = "File uploaded successfully.";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "File upload failed. Please check file type and size.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var canAccess = await _fileService.CanUserAccessFileAsync(user!.Id, id, "read", userRoles);
            if (!canAccess)
            {
                return Forbid();
            }

            var filePath = await _fileService.GetFilePhysicalPathAsync(id);
            var file = await _fileService.GetFileByIdAsync(id);

            if (file == null || string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            await _auditLogService.LogActionAsync(user.Id, "DOWNLOAD_FILE", file.FileName, GetClientIpAddress(),
                $"Downloaded file: {file.FileName}", file.FileId);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, file.FileType, file.FileName);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var canDelete = await _fileService.CanUserAccessFileAsync(user!.Id, id, "delete", userRoles);
            if (!canDelete)
            {
                TempData["Error"] = "You do not have permission to delete this file.";
                return RedirectToAction("Index");
            }

            var file = await _fileService.GetFileByIdAsync(id);
            var result = await _fileService.DeleteFileAsync(id, user.Id, userRoles);
            if (result)
            {
                await _auditLogService.LogActionAsync(user.Id, "DELETE_FILE", file?.FileName ?? $"FileId:{id}", GetClientIpAddress(),
                    $"Deleted file: {file?.FileName}", id);
                TempData["Success"] = "File deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete file.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> MyAuditLogs()
        {
            var user = await _userManager.GetUserAsync(User);
            var logs = await _auditLogService.GetAuditLogsByUserIdAsync(user!.Id, page: 1, pageSize: 50);

            return View(logs);
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}