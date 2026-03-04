using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.Books.AnyAsync())
            {
                var bookFaker = new Faker<Book>()
                    .RuleFor(b => b.Title, f => f.Commerce.ProductName())
                    .RuleFor(b => b.Author, f => f.Name.FullName())
                    .RuleFor(b => b.Isbn, f => f.Random.ReplaceNumbers("978-##########"))
                    .RuleFor(b => b.Category, f => f.PickRandom(new[] { "History", "Science", "Programming", "Fiction" }))
                    .RuleFor(b => b.IsAvailable, _ => true);

                var books = bookFaker.Generate(20);
                db.Books.AddRange(books);
                await db.SaveChangesAsync();
            }

            if (!await db.Members.AnyAsync())
            {
                var memberFaker = new Faker<Member>()
                    .RuleFor(m => m.FullName, f => f.Name.FullName())
                    .RuleFor(m => m.Email, f => f.Internet.Email())
                    .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

                var members = memberFaker.Generate(10);
                db.Members.AddRange(members);
                await db.SaveChangesAsync();
            }

            if (!await db.Loans.AnyAsync())
            {
                var books = await db.Books.ToListAsync();
                var members = await db.Members.ToListAsync();

                var rand = new Random();

                for (int i = 0; i < 15; i++)
                {
                    var book = books[rand.Next(books.Count)];
                    var member = members[rand.Next(members.Count)];

                    // avoid double-active loans during seeding
                    bool activeExists = await db.Loans.AnyAsync(l => l.BookId == book.Id && l.ReturnedDate == null);
                    if (activeExists) continue;

                    var loanDate = DateTime.UtcNow.AddDays(-rand.Next(1, 40));
                    var dueDate = loanDate.AddDays(14);

                    // mix returned/active/overdue:
                    // 0-5 returned, 6-10 active (some overdue), 11-14 active overdue
                    DateTime? returned = null;
                    if (i < 6) returned = loanDate.AddDays(rand.Next(1, 10));

                    var loan = new Loan
                    {
                        BookId = book.Id,
                        MemberId = member.Id,
                        LoanDate = loanDate,
                        DueDate = dueDate,
                        ReturnedDate = returned
                    };

                    // keep Book.IsAvailable consistent
                    book.IsAvailable = returned != null;

                    db.Loans.Add(loan);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}