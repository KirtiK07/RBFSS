using RBFSS.Models;
using RBFSS.Models.DTOs;

namespace RBFSS.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserManagementDto>> GetAllUsersAsync();
        Task<UserManagementDto?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
    }
}