using electronicLibrary.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace electronicLibrary.Data.interfaces
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetUsersAsync();
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<IdentityResult> UpdateUserRolesAsync(string userId, List<string> rolesToAdd, List<string> rolesToRemove);
        Task EnsureRolesExistAsync();
        Task<bool> IsUserInRoleAsync(string userId, string roleName);
        Task<List<ApplicationUser>> GetRegularUsersAsync();
        Task<List<IdentityRole>> GetAllRolesAsync();
    }
}
