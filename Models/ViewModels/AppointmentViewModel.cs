using System.ComponentModel.DataAnnotations;

namespace SmartCarePatientPortal.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        [Display(Name = "Reason for Visit")]
        [StringLength(500)]
        public string Reason { get; set; }

        public string? Notes { get; set; }

        public AppointmentStatus Status { get; set; }

        // For display purposes
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public string? Specialization { get; set; }

        public List<Doctor> AvailableDoctors { get; set; } = new List<Doctor>();
    }
}