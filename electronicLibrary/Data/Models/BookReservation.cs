namespace electronicLibrary.Data.Models
{
    public class BookReservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; } 
        public bool IsActive { get; set; } = true;
    }
}
