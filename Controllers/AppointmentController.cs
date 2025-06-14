using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCarePatientPortal.Models;
using SmartCarePatientPortal.Models.ViewModels;

namespace SmartCarePatientPortal.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Book()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Role != UserRole.Patient)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsAvailable)
                .ToListAsync();

            var model = new AppointmentViewModel
            {
                AvailableDoctors = doctors,
                AppointmentDate = DateTime.Today.AddDays(1),
                AppointmentTime = new TimeSpan(9, 0, 0) // Default to 9:00 AM
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Book(AppointmentViewModel model)
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
                        Notes = model.Notes,
                        Status = AppointmentStatus.Scheduled
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Appointment booked successfully!";
                    return RedirectToAction("Dashboard", "Patient");
                }
            }

            // Reload doctors if model is invalid
            model.AvailableDoctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsAvailable)
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            // Check if user has permission to view this appointment
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (user.Role == UserRole.Patient && appointment.PatientId != patient?.PatientId)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            if (user.Role == UserRole.Doctor && appointment.DoctorId != doctor?.DoctorId)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View(appointment);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Check if user has permission to cancel this appointment
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            bool hasPermission = user.Role == UserRole.Admin ||
                                (user.Role == UserRole.Patient && appointment.PatientId == patient?.PatientId) ||
                                (user.Role == UserRole.Doctor && appointment.DoctorId == doctor?.DoctorId);

            if (!hasPermission)
            {
                return Forbid();
            }

            // Only allow cancellation of scheduled or confirmed appointments
            if (appointment.Status != AppointmentStatus.Scheduled &&
                appointment.Status != AppointmentStatus.Confirmed)
            {
                TempData["Error"] = "Only scheduled appointments can be cancelled.";
                return RedirectToAction("Details", new { id });
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment cancelled successfully.";

            // Redirect based on user role
            return user.Role switch
            {
                UserRole.Patient => RedirectToAction("MyAppointments", "Patient"),
                UserRole.Doctor => RedirectToAction("Appointments", "Doctor"),
                UserRole.Admin => RedirectToAction("Reports", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Check if user has permission to update this appointment
            var user = await _userManager.GetUserAsync(User);

            if (user.Role == UserRole.Doctor)
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (appointment.DoctorId != doctor?.DoctorId)
                {
                    return Forbid();
                }
            }
            else if (user.Role != UserRole.Admin)
            {
                return Forbid();
            }

            // Update appointment status
            appointment.Status = status;
            appointment.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Appointment status updated to {status}";

            // Redirect based on user role
            return user.Role switch
            {
                UserRole.Doctor => RedirectToAction("Appointments", "Doctor"),
                UserRole.Admin => RedirectToAction("Reports", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Complete(int id)
        {
            // Quick action to mark appointment as complete
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Check if user is the doctor for this appointment
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (user.Role != UserRole.Admin && appointment.DoctorId != doctor?.DoctorId)
            {
                return Forbid();
            }

            // Mark as completed
            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Appointment with {appointment.Patient.User.FullName} marked as completed!";

            return RedirectToAction("Appointments", "Doctor");
        }
    }
}