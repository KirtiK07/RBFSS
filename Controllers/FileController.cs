using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Services.Interfaces;

namespace RBFSS.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FileController(IFileService fileService, IAuditLogService auditLogService,
            UserManager<ApplicationUser> userManager)
        {
            _fileService = fileService;
            _auditLogService = auditLogService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var canEdit = await _fileService.CanUserAccessFileAsync(user!.Id, id, "edit", userRoles);
            if (!canEdit)
            {
                TempData["Error"] = "You do not have permission to edit this file.";
                return Redirect(returnUrl ?? "/");
            }

            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null) return NotFound();

            var model = new EditFileDto
            {
                FileId = file.FileId,
                FileName = file.FileName,
                Description = file.Description,
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditFileDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var result = await _fileService.EditFileAsync(
                model.FileId, model.FileName, model.Description, user!.Id, userRoles);

            if (result)
            {
                await _auditLogService.LogActionAsync(user.Id, "EDIT_FILE",
                    model.FileName, GetClientIpAddress(),
                    $"Edited file metadata: {model.FileName}", model.FileId);

                TempData["Success"] = "File updated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update file. You may not have permission.";
            }

            return Redirect(model.ReturnUrl ?? "/");
        }

        private string GetClientIpAddress()
            => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
