using System.ComponentModel.DataAnnotations;

namespace SmartCarePatientPortal.Models.ViewModels
{
    public class PrescriptionViewModel
    {
        public int? PrescriptionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public int? AppointmentId { get; set; }

        [Required]
        [Display(Name = "Medicine Name")]
        [StringLength(100)]
        public string MedicineName { get; set; }

        [Required]
        [Display(Name = "Dosage")]
        [StringLength(50)]
        public string Dosage { get; set; }

        [Required]
        [Display(Name = "Frequency")]
        [StringLength(100)]
        public string Frequency { get; set; }

        [Required]
        [Display(Name = "Duration (days)")]
        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
        public int Duration { get; set; }

        [Display(Name = "Instructions")]
        [StringLength(500)]
        public string? Instructions { get; set; }

        public DateTime PrescribedDate { get; set; }

        // Display properties
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
    }
}