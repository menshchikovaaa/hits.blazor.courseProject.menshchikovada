using electronicLibrary.Data.interfaces;
using electronicLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace electronicLibrary.Data.Services
{
    public class BookReservationService : IBookReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookService _bookService;

        public BookReservationService(ApplicationDbContext context, IBookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        public async Task<List<BookReservation>> GetFilteredReservationsAsync(string? userId = null)
        {
            Console.WriteLine("reserv start");

            var query = _context.BookReservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.IsActive);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(r => r.UserId == userId);
            }

            return await query
                .OrderBy(r => r.ExpiryDate) 
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BookReservation> ReserveBookAsync(int bookId, string userId, int reserveDays)
        {
            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null)
                throw new KeyNotFoundException("Book not found");

            if (book.AvailableCopies <= 0)
                throw new InvalidOperationException("No available copies of this book");

            var existingReservation = await _context.BookReservations
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId && r.IsActive);

            if (existingReservation != null)
                throw new InvalidOperationException("You already have an active reservation for this book");

            var reservation = new BookReservation
            {
                BookId = bookId,
                UserId = userId,
                ReservationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(reserveDays),
                IsActive = true
            };

            await _context.BookReservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.BookReservations.FindAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            reservation.IsActive = false;
            _context.BookReservations.Update(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BookReservation>> GetUserReservationsAsync(string userId)
        {
            return await _context.BookReservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Book)
                .OrderByDescending(r => r.ReservationDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
