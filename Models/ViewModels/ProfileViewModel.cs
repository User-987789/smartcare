using System.ComponentModel.DataAnnotations;

namespace SmartCarePatientPortal.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public UserRole Role { get; set; }

        // Patient specific fields
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }
        public string? Address { get; set; }

        [Display(Name = "Emergency Contact")]
        public string? EmergencyContact { get; set; }

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        // Doctor specific fields
        public string? Specialization { get; set; }
        public string? Qualifications { get; set; }

        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Display(Name = "Experience Years")]
        public int? ExperienceYears { get; set; }
    }
}