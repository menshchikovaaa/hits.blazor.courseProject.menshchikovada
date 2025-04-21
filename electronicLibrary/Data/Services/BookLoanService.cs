using electronicLibrary.Data.interfaces;
using electronicLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace electronicLibrary.Data.Services
{
    public class BookLoanService: IBookLoanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookService _bookService;

        public BookLoanService(ApplicationDbContext context, IBookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        public async Task<BookLoan> LoanBookAsync(int bookId, string userId, int loanDays)
        {
            if (loanDays <= 0)
                throw new ArgumentException("Loan period must be positive", nameof(loanDays));

            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null)
                throw new KeyNotFoundException("Book not found");

            if (book.AvailableCopies <= 0)
                throw new InvalidOperationException("No available copies of this book");

            if (await HasUserBorrowedBookAsync(userId, bookId))
                throw new InvalidOperationException("User already has this book");

            var loan = new BookLoan
            {
                BookId = bookId,
                UserId = userId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(loanDays)
            };

            await _context.BookLoans.AddAsync(loan);

            // Обновляем количество доступных книг
            book.AvailableCopies--;
            _context.Books.Update(book);

            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task<BookLoan?> GetLoanByIdAsync(int loanId)
        {
            return await _context.BookLoans
                .Include(bl => bl.Book)
                .Include(bl => bl.User)
                .FirstOrDefaultAsync(bl => bl.Id == loanId);
        }

        public async Task<List<BookLoan>> GetActiveLoansAsync()
        {
            return await _context.BookLoans
                .Where(bl => bl.ReturnDate == null)
                .Include(bl => bl.Book)
                .Include(bl => bl.User)
                .OrderBy(bl => bl.DueDate)
                .ToListAsync();
        }

        public async Task<List<BookLoan>> GetOverdueLoansAsync()
        {
            return await _context.BookLoans
                .Where(bl => bl.ReturnDate == null && bl.DueDate < DateTime.UtcNow)
                .Include(bl => bl.Book)
                .Include(bl => bl.User)
                .OrderBy(bl => bl.DueDate)
                .ToListAsync();
        }

        public async Task<List<BookLoan>> GetUserLoansAsync(string userId)
        {
            return await _context.BookLoans
                .Where(bl => bl.UserId == userId)
                .Include(bl => bl.Book)
                .OrderByDescending(bl => bl.LoanDate)
                .ToListAsync();
        }

        public async Task<List<BookLoan>> GetUserCurrentLoansAsync(string userId)
        {
            return await _context.BookLoans
                .Where(bl => bl.UserId == userId && bl.ReturnDate == null)
                .Include(bl => bl.Book)
                .OrderBy(bl => bl.DueDate)
                .ToListAsync();
        }

        public async Task<BookLoan> ReturnBookAsync(int loanId, string? requestingUserId = null)
        {
            var loan = await _context.BookLoans
                .Include(bl => bl.Book)
                .FirstOrDefaultAsync(bl => bl.Id == loanId);

            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            if (requestingUserId != null && loan.UserId != requestingUserId)
                throw new UnauthorizedAccessException("You can only return your own books");

            if (loan.ReturnDate.HasValue)
                throw new InvalidOperationException("Book already returned");

            loan.ReturnDate = DateTime.UtcNow;
            loan.Book.AvailableCopies++;

            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task<BookLoan> RenewLoanAsync(int loanId, int additionalDays)
        {
            if (additionalDays <= 0)
                throw new ArgumentException("Additional days must be positive", nameof(additionalDays));

            var loan = await _context.BookLoans.FindAsync(loanId);
            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            if (loan.ReturnDate.HasValue)
                throw new InvalidOperationException("Cannot renew returned book");

            loan.DueDate = loan.DueDate.AddDays(additionalDays);
            _context.BookLoans.Update(loan);
            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task<bool> IsBookAvailableAsync(int bookId)
        {
            return await _context.Books
                .Where(b => b.Id == bookId && b.AvailableCopies > 0)
                .AnyAsync();
        }

        public async Task<bool> HasUserBorrowedBookAsync(string userId, int bookId)
        {
            return await _context.BookLoans
                .Where(bl => bl.UserId == userId &&
                             bl.BookId == bookId &&
                             bl.ReturnDate == null)
                .AnyAsync();
        }

        public async Task<int> GetAvailableCopiesCountAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            return book?.AvailableCopies ?? 0;
        }
    }
}
