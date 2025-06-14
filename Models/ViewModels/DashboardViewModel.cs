namespace SmartCarePatientPortal.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; }
        public UserRole UserRole { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
        public List<Appointment> UpcomingAppointments { get; set; } = new List<Appointment>();
    }
}