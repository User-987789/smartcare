using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCarePatientPortal.Models;
using SmartCarePatientPortal.Models.ViewModels;

namespace SmartCarePatientPortal.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Role != UserRole.Patient)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound();

            var model = new DashboardViewModel
            {
                UserName = user.FullName,
                UserRole = user.Role,
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .Where(a => a.PatientId == patient.PatientId && a.AppointmentDate >= DateTime.Today)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(3)
                    .ToListAsync(),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .Where(a => a.PatientId == patient.PatientId && a.AppointmentDate < DateTime.Today)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(3)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> BookAppointment()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsAvailable)
                .ToListAsync();

            var model = new AppointmentViewModel
            {
                AvailableDoctors = doctors,
                AppointmentDate = DateTime.Today.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(AppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

                // Check for conflicting appointments
                var existingAppointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.DoctorId == model.DoctorId
                                           && a.AppointmentDate.Date == model.AppointmentDate.Date
                                           && a.AppointmentTime == model.AppointmentTime
                                           && a.Status != AppointmentStatus.Cancelled);

                if (existingAppointment != null)
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please choose a different time.");
                }
                else
                {
                    var appointment = new Appointment
                    {
                        PatientId = patient.PatientId,
                        DoctorId = model.DoctorId,
                        AppointmentDate = model.AppointmentDate,
                        AppointmentTime = model.AppointmentTime,
                        Reason = model.Reason,
                        Notes = string.IsNullOrEmpty(model.Notes) ? null : model.Notes, // Handle empty notes
                        Status = AppointmentStatus.Scheduled
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Appointment booked successfully!";
                    return RedirectToAction("MyAppointments");
                }
            }

            // Reload doctors if model is invalid
            model.AvailableDoctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsAvailable)
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> MedicalRecords()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var records = await _context.MedicalRecords
                .Include(r => r.Doctor).ThenInclude(d => d.User)
                .Where(r => r.PatientId == patient.PatientId)
                .OrderByDescending(r => r.VisitDate)
                .ToListAsync();

            return View(records);
        }

        public async Task<IActionResult> Prescriptions()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Where(p => p.PatientId == patient.PatientId)
                .OrderByDescending(p => p.PrescribedDate)
                .ToListAsync();

            return View(prescriptions);
        }
    }
}