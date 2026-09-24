using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models
{
    public class AddEmployeeDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public required string Email { get; set; }

        [Phone, StringLength(25)]
        public string? Phone { get; set; }

        [Range(0, 10_000_000)]
        public decimal Salary { get; set; }
    }
}
