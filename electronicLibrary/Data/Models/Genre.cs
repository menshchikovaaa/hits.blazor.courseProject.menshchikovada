using System.ComponentModel.DataAnnotations;

namespace electronicLibrary.Data.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public List<BookGenre> BookGenres { get; set; } = new();

    }
}
