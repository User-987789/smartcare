using Microsoft.EntityFrameworkCore;
using SmartCarePatientPortal.Models;

namespace SmartCarePatientPortal.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsAsync(int? patientId = null, int? doctorId = null)
        {
            var query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.AppointmentDate >= DateTime.Today);

            if (patientId.HasValue)
                query = query.Where(a => a.PatientId == patientId.Value);

            if (doctorId.HasValue)
                query = query.Where(a => a.DoctorId == doctorId.Value);

            return await query.OrderBy(a => a.AppointmentDate).ToListAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, TimeSpan time)
        {
            return !await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId
                            && a.AppointmentDate.Date == date.Date
                            && a.AppointmentTime == time
                            && a.Status != AppointmentStatus.Cancelled);
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }
    }
}