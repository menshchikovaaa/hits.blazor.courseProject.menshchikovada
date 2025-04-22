using electronicLibrary.Data.Models;

namespace electronicLibrary.Data.interfaces
{
    public interface IBookReservationService
    {
        Task<BookReservation> ReserveBookAsync(int bookId, string userId, int reserveDays);
        Task CancelReservationAsync(int reservationId);
        Task<List<BookReservation>> GetUserReservationsAsync(string userId);
        Task<List<BookReservation>> GetFilteredReservationsAsync(string? userId = null);
    }
}
