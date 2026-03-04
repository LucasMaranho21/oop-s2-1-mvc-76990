using System;
using System.Linq;
using System.Threading.Tasks;
using Library.Domain.Entities;
using Library.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers
{
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            return View(loans);
        }

        // GET: Loans/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int memberId, int bookId)
        {
            // 1) prevent lending book already on active loan
            bool activeLoanExists = await _context.Loans
                .AnyAsync(l => l.BookId == bookId && l.ReturnedDate == null);

            if (activeLoanExists)
            {
                ModelState.AddModelError("", "This book is already on an active loan.");
            }

            // 2) ensure book exists and is available
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
            {
                ModelState.AddModelError("", "Book not found.");
            }
            else if (!book.IsAvailable)
            {
                ModelState.AddModelError("", "Book is not available.");
            }

            // 3) ensure member exists
            var memberExists = await _context.Members.AnyAsync(m => m.Id == memberId);
            if (!memberExists)
            {
                ModelState.AddModelError("", "Member not found.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View();
            }

            var loan = new Loan
            {
                MemberId = memberId,
                BookId = bookId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                ReturnedDate = null
            };

            // set book unavailable
            book!.IsAvailable = false;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Loans/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            if (loan.ReturnedDate != null)
                return RedirectToAction(nameof(Index)); // already returned

            loan.ReturnedDate = DateTime.UtcNow;

            if (loan.Book != null)
                loan.Book.IsAvailable = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync()
        {
            var members = await _context.Members
                .OrderBy(m => m.FullName)
                .ToListAsync();

            // show only available books
            var books = await _context.Books
                .Where(b => b.IsAvailable)
                .OrderBy(b => b.Title)
                .ToListAsync();

            ViewBag.Members = new SelectList(members, "Id", "FullName");
            ViewBag.Books = new SelectList(books, "Id", "Title");
        }
    }
}