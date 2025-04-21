using electronicLibrary.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace electronicLibrary.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public List<BookLoan> BookLoans { get; set; } = [];
    }

}
