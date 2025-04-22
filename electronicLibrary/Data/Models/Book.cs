using System.ComponentModel.DataAnnotations;

namespace electronicLibrary.Data.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string? Publisher { get; set; }
        public int Year { get; set; }
        public int Pages { get; set; }
        public string? Language { get; set; }
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        public List<BookAuthor> BookAuthors { get; set; } = new();
        public List<BookGenre> BookGenres { get; set; } = new();
        public List<BookLoan> BookLoans { get; set; } = new();
        public List<BookReservation> BookReservations { get; set; } = new();
    }
}
