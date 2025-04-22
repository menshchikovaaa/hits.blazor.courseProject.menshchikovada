using electronicLibrary.Data.Models;

namespace electronicLibrary.Data.interfaces
{
    public interface IBookLoanService
    {
        Task<BookLoan> LoanBookAsync(int bookId, string userId, int loanDays, bool skipReservationCheck = false);
        Task<List<BookLoan>> GetUserCurrentLoansAsync(string userId);
        Task<BookLoan> ReturnBookAsync(int loanId, string? userId = null);
        Task<BookLoan> RenewLoanAsync(int loanId, int additionalDays);
        Task<bool> HasUserBorrowedBookAsync(string userId, int bookId);
        Task<List<BookLoan>> GetFilteredLoansAsync(string? userId = null, bool overdueOnly = false);
        int GetDaysUntilDue(BookLoan loan);
        int GetOverdueDays(BookLoan loan);
    }
}
