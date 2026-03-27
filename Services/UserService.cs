using Microsoft.AspNetCore.Identity;
using RBFSS.Models;
using RBFSS.Models.DTOs;
using RBFSS.Repositories.Interfaces;
using RBFSS.Services.Interfaces;

namespace RBFSS.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserManagementDto>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<UserManagementDto?> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<bool> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new ApplicationUser
            {
                UserName = createUserDto.Email,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, createUserDto.Role);
                return true;
            }

            return false;
        }

        public async Task<bool> AssignRoleAsync(string userId, string roleName)
        {
            // Remove user from all current roles first
            var currentRoles = await _userRepository.GetUserRolesAsync(userId);
            foreach (var role in currentRoles)
            {
                await _userRepository.RemoveUserFromRoleAsync(userId, role);
            }

            // Add to new role
            return await _userRepository.AddUserToRoleAsync(userId, roleName);
        }

        public async Task<bool> RemoveRoleAsync(string userId, string roleName)
        {
            return await _userRepository.RemoveUserFromRoleAsync(userId, roleName);
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            return await _userRepository.DeactivateUserAsync(userId);
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            return await _userRepository.ActivateUserAsync(userId);
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            var userRoles = await _userRepository.GetUserRolesAsync(userId);
            return userRoles.Contains(roleName);
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            return await _userRepository.GetUserRolesAsync(userId);
        }

        public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
        {
            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var activeUsers = await _userRepository.GetActiveUsersCountAsync();

            return new Dictionary<string, int>
            {
                { "TotalUsers", totalUsers },
                { "ActiveUsers", activeUsers },
                { "InactiveUsers", totalUsers - activeUsers }
            };
        }
    }
}