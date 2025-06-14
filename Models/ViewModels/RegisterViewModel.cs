using System.ComponentModel.DataAnnotations;

namespace SmartCarePatientPortal.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }

        // Additional fields for Patient registration
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }
        public string? Address { get; set; }

        [Display(Name = "Emergency Contact")]
        public string? EmergencyContact { get; set; }

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        // Additional fields for Doctor registration
        public string? Specialization { get; set; }
        public string? Qualifications { get; set; }

        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Display(Name = "Experience Years")]
        public int? ExperienceYears { get; set; }
    }
}