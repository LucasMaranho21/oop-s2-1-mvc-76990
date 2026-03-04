using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Author { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Isbn { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        // Navigation: Book 1 -> * Loans
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}