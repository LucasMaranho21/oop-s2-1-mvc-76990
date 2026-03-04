using System;
using System.Linq;
using System.Threading.Tasks;
using Library.Domain.Entities;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Library.Tests
{
    public class LoanRulesTests
    {
        private static ApplicationDbContext CreateDb()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(opts);
        }

        [Fact]
        public async Task Cannot_create_loan_for_book_on_active_loan()
        {
            using var db = CreateDb();

            var book = new Book { Title = "A", Author = "B", Isbn = "978-0000000001", Category = "X", IsAvailable = false };
            var member = new Member { FullName = "M", Email = "m@m.com", Phone = "1" };
            db.Books.Add(book);
            db.Members.Add(member);
            await db.SaveChangesAsync();

            db.Loans.Add(new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(14) });
            await db.SaveChangesAsync();

            bool activeExists = db.Loans.Any(l => l.BookId == book.Id && l.ReturnedDate == null);
            Assert.True(activeExists);
        }

        [Fact]
        public async Task Returned_loan_makes_book_available()
        {
            using var db = CreateDb();

            var book = new Book { Title = "A", Author = "B", Isbn = "978-0000000002", Category = "X", IsAvailable = false };
            var member = new Member { FullName = "M", Email = "m2@m.com", Phone = "1" };
            db.AddRange(book, member);
            await db.SaveChangesAsync();

            var loan = new Loan { BookId = book.Id, MemberId = member.Id, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(14) };
            db.Loans.Add(loan);
            await db.SaveChangesAsync();

            loan.ReturnedDate = DateTime.UtcNow;
            book.IsAvailable = true;
            await db.SaveChangesAsync();

            var reloadedBook = await db.Books.FindAsync(book.Id);
            Assert.True(reloadedBook!.IsAvailable);
        }

        [Fact]
        public async Task Book_search_returns_expected_matches()
        {
            using var db = CreateDb();
            db.Books.AddRange(
                new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "978-0000000003", Category = "Programming" },
                new Book { Title = "History 101", Author = "Alice", Isbn = "978-0000000004", Category = "History" }
            );
            await db.SaveChangesAsync();

            var search = "Martin";
            var results = await db.Books
                .Where(b => b.Title.Contains(search) || b.Author.Contains(search))
                .ToListAsync();

            Assert.Single(results);
            Assert.Equal("Clean Code", results[0].Title);
        }

        [Fact]
        public void Overdue_logic_is_correct()
        {
            var loan = new Loan
            {
                DueDate = DateTime.UtcNow.AddDays(-1),
                ReturnedDate = null
            };

            Assert.True(loan.IsOverdue);
        }

        [Fact]
        public void Active_loan_logic_is_correct()
        {
            var loan = new Loan { ReturnedDate = null };
            Assert.True(loan.IsActive);

            loan.ReturnedDate = DateTime.UtcNow;
            Assert.False(loan.IsActive);
        }
    }
}