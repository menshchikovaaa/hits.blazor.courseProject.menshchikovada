using electronicLibrary.Data.Models;

namespace electronicLibrary.Data.interfaces
{
    public interface IAuthorService
    {
        Task<List<Author>> GetAuthorsAsync();
        Task<Author?> GetAuthorByIdAsync(int id);
        Task<List<Author>> SearchAuthorsAsync(string searchTerm);
        Task AddAuthorAsync(Author author);
        Task UpdateAuthorAsync(Author author);
        Task DeleteAuthorAsync(int id);
        Task<List<Book>> GetBooksByAuthorAsync(int authorId);
        Task<bool> AuthorExistsAsync(int id);
        Task<bool> AuthorHasBooksAsync(int authorId);
    }
}
