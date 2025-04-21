using electronicLibrary.Data.interfaces;
using electronicLibrary.Data.Models;
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

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.Users
                .Include(u => u.BookLoans)
                .ThenInclude(bl => bl.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException("Пользователь не найден");

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.UserName = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;

            return await _userManager.UpdateAsync(existingUser);
        }

        public async Task<IdentityResult> DeleteUserAsync(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.BookLoans)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            if (user.BookLoans?.Any(bl => !bl.ReturnDate.HasValue) == true)
                throw new InvalidOperationException("Нельзя удалить пользователя с активными займами");

            return await _userManager.DeleteAsync(user);
        }

        public async Task<List<BookLoan>> GetUserLoansAsync(string userId)
        {
            return await _context.BookLoans
                .Where(bl => bl.UserId == userId)
                .Include(bl => bl.Book)
                .OrderByDescending(bl => bl.LoanDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<BookLoan>> GetUserOverdueLoansAsync(string userId)
        {
            return await _context.BookLoans
                .Where(bl => bl.UserId == userId &&
                             bl.IsOverdue)
                .Include(bl => bl.Book)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetUsersCountAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            var activePeriod = DateTime.UtcNow.AddMonths(-3);
            return await _userManager.Users
                .Where(u => u.BookLoans.Any(bl => bl.LoanDate >= activePeriod))
                .CountAsync();
        }

        public async Task<bool> IsUserLibrarianAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, "Librarian");
        }

        public async Task<IdentityResult> SetUserAsLibrarianAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            if (!await _roleManager.RoleExistsAsync("Librarian"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Librarian"));
            }

            return await _userManager.AddToRoleAsync(user, "Librarian");
        }

        public async Task<IdentityResult> RemoveUserFromLibrarianRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            return await _userManager.RemoveFromRoleAsync(user, "Librarian");
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
    }
}
