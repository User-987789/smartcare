using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartCarePatientPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual Patient? Patient { get; set; }
        public virtual Doctor? Doctor { get; set; }
    }

    public enum UserRole
    {
        Admin = 1,
        Doctor = 2,
        Patient = 3
    }
}
