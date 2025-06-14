using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarePatientPortal.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string PatientNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string EmergencyContact { get; set; }

        [StringLength(10)]
        public string BloodGroup { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}