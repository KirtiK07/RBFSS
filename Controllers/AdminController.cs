using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Services.Interfaces;

namespace RBFSS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IFileService _fileService;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IUserService userService, IFileService fileService, IAuditLogService auditLogService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _fileService = fileService;
            _auditLogService = auditLogService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userStats = await _userService.GetUserStatisticsAsync();
            var fileStats = await _fileService.GetFileStatisticsAsync();
            var auditStats = await _auditLogService.GetAuditStatisticsAsync();
            var recentAuditLogs = await _auditLogService.GetRecentAuditLogsAsync(10);

            var model = new AdminDashboardViewModel
            {
                UserStatistics = userStats,
                FileStatistics = fileStats,
                AuditStatistics = auditStats,
                RecentAuditLogs = recentAuditLogs
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Files()
        {
            var files = await _fileService.GetAllFilesAsync();
            return View(files);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.CreateUserAsync(model);
                if (result)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        await _auditLogService.LogActionAsync(currentUser.Id, "CREATE_USER", $"Created user: {model.Email}", GetClientIpAddress());
                    }
                    return RedirectToAction("Users");
                }
                ModelState.AddModelError("", "Failed to create user");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var result = await _userService.AssignRoleAsync(userId, roleName);
            if (result)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await _auditLogService.LogActionAsync(currentUser.Id, "ASSIGN_ROLE", $"Assigned role {roleName} to user {userId}", GetClientIpAddress());
                }
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            var result = await _userService.DeactivateUserAsync(userId);
            if (result)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    await _auditLogService.LogActionAsync(currentUser.Id, "DEACTIVATE_USER", $"Deactivated user {userId}", GetClientIpAddress());
                }
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser!);

            var result = await _fileService.DeleteFileAsync(fileId, currentUser!.Id, userRoles);
            if (result)
            {
                await _auditLogService.LogActionAsync(currentUser.Id, "DELETE_FILE", $"Deleted file {fileId}", GetClientIpAddress(), fileId: fileId);
            }

            return RedirectToAction("Files");
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogs(int page = 1)
        {
            var logs = await _auditLogService.SearchAuditLogsAsync(page: page, pageSize: 50);
            return View(logs);
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }

    public class AdminDashboardViewModel
    {
        public Dictionary<string, int> UserStatistics { get; set; } = new();
        public Dictionary<string, object> FileStatistics { get; set; } = new();
        public Dictionary<string, int> AuditStatistics { get; set; } = new();
        public IEnumerable<AuditLog> RecentAuditLogs { get; set; } = new List<AuditLog>();
    }
}