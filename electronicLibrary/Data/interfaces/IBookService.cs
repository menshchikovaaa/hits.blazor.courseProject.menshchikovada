using electronicLibrary.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace electronicLibrary.Data.interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<List<Book>> SearchBooksAsync(string searchTerm);
        Task<List<Book>> GetBooksByAuthorAsync(int authorId);
        Task<List<Book>> GetBooksByGenreAsync(int genreId);
        Task AddBookAsync(Book book, List<int> authorIds, List<int> genreIds);
        Task UpdateBookAsync(Book book, List<int> authorIds, List<int> genreIds);
        Task DeleteBookAsync(int id);
        Task<bool> CheckAvailabilityAsync(int bookId);
        Task<int> GetAvailableCopiesCountAsync(int bookId);
        Task<List<Author>> GetBookAuthorsAsync(int bookId);
        Task<List<Genre>> GetBookGenresAsync(int bookId);
    }
}