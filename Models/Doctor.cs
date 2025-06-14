using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarePatientPortal.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string DoctorNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; }

        [StringLength(200)]
        public string Qualifications { get; set; }

        [StringLength(20)]
        public string LicenseNumber { get; set; }

        public int ExperienceYears { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}