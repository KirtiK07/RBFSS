using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Services.Interfaces;

namespace RBFSS.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerController : Controller
    {
        private readonly IFileService _fileService;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ManagerController(IFileService fileService, IAuditLogService auditLogService, IUserService userService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _fileService = fileService;
            _auditLogService = auditLogService;
            _userService = userService;
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
                var fullUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsPath, "manager");

                // If sharing with all users, resolve all user IDs now
                if (model.ShareWithAllUsers)
                {
                    var allUsers = await _userService.GetAllUsersAsync();
                    model.SharedWithUserIds = allUsers
                        .Where(u => u.UserId != user!.Id) // exclude uploader
                        .Select(u => u.UserId)
                        .ToList();
                }

                var uploadedFile = await _fileService.UploadFileAsync(model, user!.Id, fullUploadsPath);

                if (uploadedFile != null)
                {
                    await _auditLogService.LogActionAsync(user.Id, "UPLOAD_FILE", uploadedFile.FileName, GetClientIpAddress(),
                        $"Uploaded file: {uploadedFile.FileName}", uploadedFile.FileId);

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

            var canAccess = await _fileService.CanUserAccessFileAsync(user.Id, id, "read", userRoles);
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

        [HttpGet]
        public async Task<IActionResult> MyFiles()
        {
            var user = await _userManager.GetUserAsync(User);
            var files = await _fileService.GetUserFilesAsync(user!.Id, new[] { "Manager" });

            return View(files.Where(f => f.UploadedById == user.Id));
        }

        [HttpGet]
        public async Task<IActionResult> SharedFiles()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var files = await _fileService.GetUserFilesAsync(user.Id, userRoles);

            return View(files.Where(f => f.UploadedById != user.Id));
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}