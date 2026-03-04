using System;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }

        // FK -> Book
        [Required]
        public int BookId { get; set; }
        public Book? Book { get; set; }

        // FK -> Member
        [Required]
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public DateTime LoanDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);

        // null = active loan
        public DateTime? ReturnedDate { get; set; }

        public bool IsActive => ReturnedDate == null;
        public bool IsOverdue => ReturnedDate == null && DueDate.Date < DateTime.UtcNow.Date;
    }
}