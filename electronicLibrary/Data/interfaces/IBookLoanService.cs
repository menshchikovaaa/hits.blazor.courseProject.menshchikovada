using electronicLibrary.Data.Models;

namespace electronicLibrary.Data.interfaces
{
    public interface IBookLoanService
    {
        Task<BookLoan> LoanBookAsync(int bookId, string userId, int loanDays);
        Task<BookLoan?> GetLoanByIdAsync(int loanId);
        Task<List<BookLoan>> GetActiveLoansAsync();
        Task<List<BookLoan>> GetOverdueLoansAsync();
        Task<List<BookLoan>> GetUserLoansAsync(string userId);
        Task<List<BookLoan>> GetUserCurrentLoansAsync(string userId);
        Task<BookLoan> ReturnBookAsync(int loanId, string? userId = null);
        Task<BookLoan> RenewLoanAsync(int loanId, int additionalDays);
        Task<bool> IsBookAvailableAsync(int bookId);
        Task<bool> HasUserBorrowedBookAsync(string userId, int bookId);
        Task<int> GetAvailableCopiesCountAsync(int bookId);
    }
}
