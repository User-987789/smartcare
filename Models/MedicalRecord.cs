using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarePatientPortal.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Diagnosis { get; set; }

        // FIXED: Optional fields are now nullable
        [StringLength(500)]
        public string? Symptoms { get; set; }

        [StringLength(1000)]
        public string? Treatment { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
    }
}