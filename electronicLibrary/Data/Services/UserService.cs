using electronicLibrary.Data.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace electronicLibrary.Data.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users
                .Include(u => u.BookLoans)
                .ThenInclude(bl => bl.Book)
                .OrderBy(u => u.FullName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            return result;
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> UpdateUserRolesAsync(string userId, List<string> rolesToAdd, List<string> rolesToRemove)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Пользователь не найден" });

            await EnsureRolesExistAsync();

            foreach (var role in rolesToRemove)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, role);
                    if (!result.Succeeded)
                        return result;
                }
            }

            foreach (var role in rolesToAdd)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                        return result;
                }
            }

            return IdentityResult.Success;
        }

        public async Task EnsureRolesExistAsync()
        {
            var requiredRoles = new[] { "User", "Librarian", "Admin" };

            foreach (var roleName in requiredRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<List<ApplicationUser>> GetRegularUsersAsync()
        {
            return (await _userManager.GetUsersInRoleAsync("User"))
                .OrderBy(u => u.FullName)
                .ToList();
        }

        public async Task<List<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles
                .OrderBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
