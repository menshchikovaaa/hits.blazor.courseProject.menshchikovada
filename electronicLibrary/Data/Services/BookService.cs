using electronicLibrary.Data;
using electronicLibrary.Data.interfaces;
using electronicLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace electronicLibrary.Data.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetBooksAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .Where(b => b.Title.ToLower().Contains(searchTerm) ||
                             b.ISBN.ToLower().Contains(searchTerm) ||
                             b.Publisher.ToLower().Contains(searchTerm) ||
                             b.BookAuthors.Any(ba => ba.Author.FullName.ToLower().Contains(searchTerm)))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Book>> GetBooksByGenreAsync(int genreId)
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddBookAsync(Book book, List<int> authorIds, List<int> genreIds)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            // Проверка уникальности ISBN
            if (await _context.Books.AnyAsync(b => b.ISBN == book.ISBN))
                throw new InvalidOperationException("Книга с таким ISBN уже существует");

            // Добавление книги
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            // Добавление связей с авторами
            foreach (var authorId in authorIds)
            {
                if (!await _context.Authors.AnyAsync(a => a.Id == authorId))
                    continue;

                _context.BookAuthors.Add(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = authorId
                });
            }

            // Добавление связей с жанрами
            foreach (var genreId in genreIds)
            {
                if (!await _context.Genres.AnyAsync(g => g.Id == genreId))
                    continue;

                _context.BookGenres.Add(new BookGenre
                {
                    BookId = book.Id,
                    GenreId = genreId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book, List<int> authorIds, List<int> genreIds)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            var existingBook = await _context.Books
                .Include(b => b.BookAuthors)
                .Include(b => b.BookGenres)
                .FirstOrDefaultAsync(b => b.Id == book.Id);

            if (existingBook == null)
                throw new KeyNotFoundException("Книга не найдена");

            // Проверка уникальности ISBN (исключая текущую книгу)
            if (await _context.Books.AnyAsync(b => b.ISBN == book.ISBN && b.Id != book.Id))
                throw new InvalidOperationException("Книга с таким ISBN уже существует");

            // Обновление свойств книги
            _context.Entry(existingBook).CurrentValues.SetValues(book);

            // Обновление авторов
            UpdateBookAuthors(existingBook, authorIds);

            // Обновление жанров
            UpdateBookGenres(existingBook, genreIds);

            await _context.SaveChangesAsync();
        }

        private void UpdateBookAuthors(Book book, List<int> authorIds)
        {
            // Удаление отсутствующих связей
            var authorsToRemove = book.BookAuthors
                .Where(ba => !authorIds.Contains(ba.AuthorId))
                .ToList();

            foreach (var authorToRemove in authorsToRemove)
            {
                _context.BookAuthors.Remove(authorToRemove);
            }

            // Добавление новых связей
            var existingAuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToList();
            var authorsToAdd = authorIds
                .Where(id => !existingAuthorIds.Contains(id))
                .Select(id => new BookAuthor { BookId = book.Id, AuthorId = id });

            foreach (var authorToAdd in authorsToAdd)
            {
                _context.BookAuthors.Add(authorToAdd);
            }
        }

        private void UpdateBookGenres(Book book, List<int> genreIds)
        {
            // Удаление отсутствующих связей
            var genresToRemove = book.BookGenres
                .Where(bg => !genreIds.Contains(bg.GenreId))
                .ToList();

            foreach (var genreToRemove in genresToRemove)
            {
                _context.BookGenres.Remove(genreToRemove);
            }

            // Добавление новых связей
            var existingGenreIds = book.BookGenres.Select(bg => bg.GenreId).ToList();
            var genresToAdd = genreIds
                .Where(id => !existingGenreIds.Contains(id))
                .Select(id => new BookGenre { BookId = book.Id, GenreId = id });

            foreach (var genreToAdd in genresToAdd)
            {
                _context.BookGenres.Add(genreToAdd);
            }
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .Include(b => b.BookGenres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                throw new KeyNotFoundException("Книга не найдена");

            // Проверка на наличие выданных экземпляров
            if (book.TotalCopies != book.AvailableCopies)
                throw new InvalidOperationException("Нельзя удалить книгу, так как есть выданные экземпляры");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckAvailabilityAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            return book?.AvailableCopies > 0;
        }

        public async Task<int> GetAvailableCopiesCountAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            return book?.AvailableCopies ?? 0;
        }

        public async Task<List<Author>> GetBookAuthorsAsync(int bookId)
        {
            return await _context.BookAuthors
                .Where(ba => ba.BookId == bookId)
                .Select(ba => ba.Author)
                .ToListAsync();
        }

        public async Task<List<Genre>> GetBookGenresAsync(int bookId)
        {
            return await _context.BookGenres
                .Where(bg => bg.BookId == bookId)
                .Select(bg => bg.Genre)
                .ToListAsync();
        }
    }
}