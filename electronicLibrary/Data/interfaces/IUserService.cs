using electronicLibrary.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace electronicLibrary.Data.interfaces
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> DeleteUserAsync(string id);
        Task<List<BookLoan>> GetUserLoansAsync(string userId);
        Task<List<BookLoan>> GetUserOverdueLoansAsync(string userId);
        Task<int> GetUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<bool> IsUserLibrarianAsync(string userId);
        Task<IdentityResult> SetUserAsLibrarianAsync(string userId);
        Task<IdentityResult> RemoveUserFromLibrarianRoleAsync(string userId);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);

        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<IdentityResult> UpdateUserRolesAsync(string userId, List<string> rolesToAdd, List<string> rolesToRemove);
        Task EnsureRolesExistAsync();
    }
}
