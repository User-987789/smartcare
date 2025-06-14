using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCarePatientPortal.Models;
using SmartCarePatientPortal.Models.ViewModels;

namespace SmartCarePatientPortal.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Role != UserRole.Admin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var model = new DashboardViewModel
            {
                UserName = user.FullName,
                UserRole = user.Role,
                TotalPatients = await _context.Patients.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == DateTime.Today),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Reports()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .ToListAsync();

            return View(appointments);
        }
    }
}