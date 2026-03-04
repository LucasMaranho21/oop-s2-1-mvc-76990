using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities
{
    public class Member
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Phone { get; set; } = string.Empty;

        // Navigation: Member 1 -> * Loans
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}