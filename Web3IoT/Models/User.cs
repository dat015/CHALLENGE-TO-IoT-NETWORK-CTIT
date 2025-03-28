using System.ComponentModel.DataAnnotations;

namespace Web3IoT.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public required string Address { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();
    }
} 