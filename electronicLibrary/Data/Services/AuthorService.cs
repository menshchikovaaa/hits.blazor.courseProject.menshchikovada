using electronicLibrary.Data.interfaces;
using electronicLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace electronicLibrary.Data.Services
{
    public class AuthorService : IAuthorService
    {

        private readonly ApplicationDbContext _context;

        public AuthorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Author>> GetAuthorsAsync()
        {
            return await _context.Authors
                .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
                .OrderBy(a => a.FullName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Author?> GetAuthorByIdAsync(int id)
        {
            return await _context.Authors
                .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Author>> SearchAuthorsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAuthorsAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Authors
                .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
                .Where(a => a.FullName.ToLower().Contains(searchTerm) ||
                                a.Biography != null && a.Biography.ToLower().Contains(searchTerm))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAuthorAsync(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            // Проверка на уникальность имени (опционально)
            if (await _context.Authors.AnyAsync(a => a.FullName == author.FullName))
                throw new InvalidOperationException("Автор с таким именем уже существует");

            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAuthorAsync(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            var existingAuthor = await _context.Authors.FindAsync(author.Id);
            if (existingAuthor == null)
                throw new KeyNotFoundException("Автор не найден");

            // Проверка на уникальность имени (опционально)
            if (await _context.Authors.AnyAsync(a => a.FullName == author.FullName && a.Id != author.Id))
                throw new InvalidOperationException("Автор с таким именем уже существует");

            _context.Entry(existingAuthor).CurrentValues.SetValues(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAuthorAsync(int id)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                throw new KeyNotFoundException("Автор не найден");

            if (author.BookAuthors?.Any() == true)
                throw new InvalidOperationException("Нельзя удалить автора, у которого есть книги");

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _context.BookAuthors
                .Where(ba => ba.AuthorId == authorId)
                .Include(ba => ba.Book)
                .ThenInclude(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Select(ba => ba.Book)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AuthorExistsAsync(int id)
        {
            return await _context.Authors.AnyAsync(a => a.Id == id);
        }

        public async Task<bool> AuthorHasBooksAsync(int authorId)
        {
            return await _context.BookAuthors.AnyAsync(ba => ba.AuthorId == authorId);
        }
    }
}


