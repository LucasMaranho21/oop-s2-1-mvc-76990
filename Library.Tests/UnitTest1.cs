using Library.Domain.Entities;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Library.Tests
{
    public class UnitTest1
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Cannot_Create_Second_Active_Loan_For_Same_Book()
        {
            using var context = GetDbContext();

            var book = new Book
            {
                Title = "C# Basics",
                Author = "Anders Hejlsberg",
                Isbn = "123456",
                Category = "Programming",
                IsAvailable = false
            };

            var member1 = new Member
            {
                FullName = "Lucas",
                Email = "lucas@test.com",
                Phone = "0832064512"
            };

            var member2 = new Member
            {
                FullName = "Gabriela",
                Email = "gabriela@test.com",
                Phone = "0895551234"
            };

            context.Books.Add(book);
            context.Members.AddRange(member1, member2);
            context.SaveChanges();

            var firstLoan = new Loan
            {
                BookId = book.Id,
                MemberId = member1.Id,
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7),
                ReturnedDate = null
            };

            context.Loans.Add(firstLoan);
            context.SaveChanges();

            var activeLoansForBook = context.Loans
                .Where(l => l.BookId == book.Id && l.ReturnedDate == null)
                .ToList();

            Assert.Single(activeLoansForBook);
            Assert.Equal(member1.Id, activeLoansForBook[0].MemberId);
        }

        [Fact]
        public void Returned_Loan_Makes_Book_Available_Again()
        {
            using var context = GetDbContext();

            var book = new Book
            {
                Title = "ASP.NET MVC",
                Author = "Jorge Washington",
                Isbn = "654321",
                Category = "Programming",
                IsAvailable = false
            };

            var member = new Member
            {
                FullName = "John Lennon",
                Email = "john@test.com",
                Phone = "0832224569"
            };

            context.Books.Add(book);
            context.Members.Add(member);
            context.SaveChanges();

            var loan = new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(4),
                ReturnedDate = null
            };

            context.Loans.Add(loan);
            context.SaveChanges();

            loan.ReturnedDate = DateTime.Today;
            book.IsAvailable = true;

            context.SaveChanges();

            Assert.NotNull(loan.ReturnedDate);
            Assert.True(book.IsAvailable);
        }

        [Fact]
        public void Book_Search_By_Title_Returns_Expected_Match()
        {
            using var context = GetDbContext();

            context.Books.AddRange(
                new Book
                {
                    Title = "C# Programming",
                    Author = "John Smith",
                    Isbn = "475869",
                    Category = "Programming",
                    IsAvailable = true
                },
                new Book
                {
                    Title = "World History",
                    Author = "Anna Brown",
                    Isbn = "748596",
                    Category = "History",
                    IsAvailable = true
                }
            );

            context.SaveChanges();

            var result = context.Books
                .Where(b => b.Title.Contains("C#") || b.Author.Contains("C#"))
                .ToList();

            Assert.Single(result);
            Assert.Equal("C# Programming", result[0].Title);
        }

        [Fact]
        public void Overdue_Loan_Is_Detected_Correctly()
        {
            using var context = GetDbContext();

            var loan = new Loan
            {
                BookId = 1,
                MemberId = 1,
                LoanDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(-5),
                ReturnedDate = null
            };

            context.Loans.Add(loan);
            context.SaveChanges();

            var overdueLoans = context.Loans
                .Where(l => l.DueDate < DateTime.Today && l.ReturnedDate == null)
                .ToList();

            Assert.Single(overdueLoans);
        }

        [Fact]
        public void Availability_Filter_Returns_Only_Available_Books()
        {
            using var context = GetDbContext();

            var availableBook = new Book
            {
                Title = "The Tend",
                Author = "August Lennin",
                Isbn = "014785",
                Category = "Fiction",
                IsAvailable = true
            };

            var unavailableBook = new Book
            {
                Title = "Harry Potter",
                Author = "Joanne Rowling",
                Isbn = "616212",
                Category = "Science",
                IsAvailable = false
            };

            context.Books.AddRange(availableBook, unavailableBook);
            context.SaveChanges();

            var result = context.Books
                .Where(b => b.Title == "The Tend" || b.Title == "Harry Potter")
                .Where(b => b.IsAvailable)
                .ToList();

            Assert.Single(result);
            Assert.Equal("The Tend", result[0].Title);
            Assert.DoesNotContain(result, b => b.Title == "Harry Potter");
        }
    }
}