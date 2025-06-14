using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCarePatientPortal.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        // FIXED: Notes is now optional (nullable)
        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
    }

    public enum AppointmentStatus
    {
        Scheduled = 1,
        Confirmed = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }
}