using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarePatientPortal.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(100)]
        public string MedicineName { get; set; }

        [Required]
        [StringLength(50)]
        public string Dosage { get; set; }

        [Required]
        [StringLength(100)]
        public string Frequency { get; set; }

        [Required]
        public int Duration { get; set; } // in days

        [StringLength(500)]
        public string Instructions { get; set; }

        public DateTime PrescribedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
    }
}