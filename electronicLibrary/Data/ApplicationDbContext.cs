using electronicLibrary.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace electronicLibrary.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public new DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookLoan> BookLoans { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<BookReservation> BookReservations { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BookLoan>(entity =>
            {
                entity.HasOne(bl => bl.User)
                      .WithMany(u => u.BookLoans)
                      .HasForeignKey(bl => bl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bl => bl.Book)
                      .WithMany(b => b.BookLoans)
                      .HasForeignKey(bl => bl.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BookReservation>(entity =>
            {
                entity.HasOne(br => br.User)
                      .WithMany(u => u.BookReservations)
                      .HasForeignKey(br => br.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(br => br.Book)
                      .WithMany(b => b.BookReservations)
                      .HasForeignKey(br => br.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка связей многие-ко-многим (Book - Author)
            builder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            builder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);

            builder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            // Настройка связей многие-ко-многим (Book - Genre)
            builder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });

            builder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId);

            builder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId);

            // Уникальные индексы
            builder.Entity<Book>()
                .HasIndex(b => b.ISBN)
            .IsUnique();
        }
    }
}
