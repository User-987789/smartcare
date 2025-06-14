using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCarePatientPortal.Models;
using SmartCarePatientPortal.Models.ViewModels;

namespace SmartCarePatientPortal.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Role != UserRole.Doctor)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound();

            var model = new DashboardViewModel
            {
                UserName = user.FullName,
                UserRole = user.Role,
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.DoctorId && a.AppointmentDate.Date == DateTime.Today),
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .Where(a => a.DoctorId == doctor.DoctorId && a.AppointmentDate >= DateTime.Today)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Patients()
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            var patients = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .Where(p => p.Appointments.Any(a => a.DoctorId == doctor.DoctorId))
                .ToListAsync();

            return View(patients);
        }

        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> AddRecord(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null) return NotFound();

            ViewBag.Patient = patient;
            return View(new MedicalRecord { PatientId = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> AddRecord(MedicalRecord record)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            record.DoctorId = doctor.DoctorId;
            record.VisitDate = DateTime.Now;

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return RedirectToAction("Patients");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> QuickComplete(int appointmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctor.DoctorId);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found or you don't have permission.";
                return RedirectToAction("Dashboard");
            }

            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment completed successfully!";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddPrescription(int patientId, int? appointmentId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null || doctor == null)
            {
                return NotFound();
            }

            var model = new PrescriptionViewModel
            {
                PatientId = patientId,
                DoctorId = doctor.DoctorId,
                PatientName = patient.User.FullName,
                DoctorName = doctor.User.FullName,
                AppointmentId = appointmentId,
                PrescribedDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPrescription(PrescriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

                var prescription = new Prescription
                {
                    PatientId = model.PatientId,
                    DoctorId = doctor.DoctorId,
                    MedicineName = model.MedicineName,
                    Dosage = model.Dosage,
                    Frequency = model.Frequency,
                    Duration = model.Duration,
                    Instructions = model.Instructions,
                    PrescribedDate = DateTime.Now
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Prescription for {model.MedicineName} added successfully!";

                // If came from appointment, redirect back to appointment details
                if (model.AppointmentId.HasValue)
                {
                    return RedirectToAction("Details", "Appointment", new { id = model.AppointmentId });
                }

                return RedirectToAction("Patients");
            }

            // Reload patient info if model is invalid
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == model.PatientId);

            if (patient != null)
            {
                model.PatientName = patient.User.FullName;
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewPrescriptions(int patientId)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.PrescribedDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            return View(prescriptions);
        }
    }
}