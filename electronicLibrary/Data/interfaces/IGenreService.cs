using electronicLibrary.Data.Models;

namespace electronicLibrary.Data.interfaces
{
    public interface IGenreService
    {
        Task<List<Genre>> GetGenresAsync();
        Task<Genre?> GetGenreByIdAsync(int id);
        Task<List<Genre>> SearchGenresAsync(string searchTerm);
        Task AddGenreAsync(Genre genre);
        Task UpdateGenreAsync(Genre genre);
        Task DeleteGenreAsync(int id);
        Task<List<Book>> GetBooksByGenreAsync(int genreId);
        Task<bool> GenreExistsAsync(int id);
        Task<bool> GenreHasBooksAsync(int genreId);

    }
}
