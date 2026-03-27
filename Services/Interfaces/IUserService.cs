using RBFSS.Models;
using RBFSS.Models.DTOs;

namespace RBFSS.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserManagementDto>> GetAllUsersAsync();
        Task<UserManagementDto?> GetUserByIdAsync(string userId);
        Task<bool> CreateUserAsync(CreateUserDto createUserDto);
        Task<bool> AssignRoleAsync(string userId, string roleName);
        Task<bool> RemoveRoleAsync(string userId, string roleName);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
        Task<bool> IsUserInRoleAsync(string userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<Dictionary<string, int>> GetUserStatisticsAsync();
    }
}