using System.ComponentModel.DataAnnotations;

namespace electronicLibrary.Data.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Biography { get; set; }

        public List<BookAuthor> BookAuthors { get; set; } = new();

    }
}
