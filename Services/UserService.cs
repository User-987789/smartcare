using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCarePatientPortal.Models;

namespace SmartCarePatientPortal.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<Patient?> GetPatientByUserIdAsync(string userId)
        {
            return await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Doctor?> GetDoctorByUserIdAsync(string userId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Doctor)
                .ToListAsync();
        }
    }
}