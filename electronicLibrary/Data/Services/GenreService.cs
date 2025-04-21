using electronicLibrary.Data.interfaces;
using electronicLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace electronicLibrary.Data.Services
{
    public class GenreService : IGenreService
    {
        private readonly ApplicationDbContext _context;

        public GenreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Genre>> GetGenresAsync()
        {
            return await _context.Genres
                .Include(g => g.BookGenres)
                .ThenInclude(bg => bg.Book)
                .OrderBy(g => g.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Genre?> GetGenreByIdAsync(int id)
        {
            return await _context.Genres
                .Include(g => g.BookGenres)
                .ThenInclude(bg => bg.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<Genre>> SearchGenresAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetGenresAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Genres
                .Include(g => g.BookGenres)
                .ThenInclude(bg => bg.Book)
                .Where(g => g.Name.ToLower().Contains(searchTerm) ||
                           (g.Description != null && g.Description.ToLower().Contains(searchTerm)))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddGenreAsync(Genre genre)
        {
            if (genre == null)
                throw new ArgumentNullException(nameof(genre));

            // Проверка на уникальность имени
            if (await _context.Genres.AnyAsync(g => g.Name == genre.Name))
                throw new InvalidOperationException("Жанр с таким названием уже существует");

            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGenreAsync(Genre genre)
        {
            if (genre == null)
                throw new ArgumentNullException(nameof(genre));

            var existingGenre = await _context.Genres.FindAsync(genre.Id);
            if (existingGenre == null)
                throw new KeyNotFoundException("Жанр не найден");

            // Проверка на уникальность имени
            if (await _context.Genres.AnyAsync(g => g.Name == genre.Name && g.Id != genre.Id))
                throw new InvalidOperationException("Жанр с таким названием уже существует");

            _context.Entry(existingGenre).CurrentValues.SetValues(genre);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGenreAsync(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.BookGenres)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
                throw new KeyNotFoundException("Жанр не найден");

            if (genre.BookGenres?.Any() == true)
                throw new InvalidOperationException("Нельзя удалить жанр, у которого есть книги");

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Book>> GetBooksByGenreAsync(int genreId)
        {
            return await _context.BookGenres
                .Where(bg => bg.GenreId == genreId)
                .Include(bg => bg.Book)
                .ThenInclude(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .Select(bg => bg.Book)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> GenreExistsAsync(int id)
        {
            return await _context.Genres.AnyAsync(g => g.Id == id);
        }

        public async Task<bool> GenreHasBooksAsync(int genreId)
        {
            return await _context.BookGenres.AnyAsync(bg => bg.GenreId == genreId);
        }
    }
}
