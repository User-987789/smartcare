using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCarePatientPortal.Models;
using SmartCarePatientPortal.Models.ViewModels;

namespace SmartCarePatientPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
                               SignInManager<ApplicationUser> signInManager,
                               ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    // Redirect based on user role
                    return user.Role switch
                    {
                        UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                        UserRole.Doctor => RedirectToAction("Dashboard", "Doctor"),
                        UserRole.Patient => RedirectToAction("Dashboard", "Patient"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Create role-specific records
                    if (model.Role == UserRole.Patient)
                    {
                        var patient = new Patient
                        {
                            UserId = user.Id,
                            PatientNumber = "P" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                            Gender = model.Gender,
                            Address = model.Address,
                            EmergencyContact = model.EmergencyContact,
                            BloodGroup = model.BloodGroup
                        };
                        _context.Patients.Add(patient);
                    }
                    else if (model.Role == UserRole.Doctor)
                    {
                        var doctor = new Doctor
                        {
                            UserId = user.Id,
                            DoctorNumber = "D" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            Specialization = model.Specialization ?? "General Medicine",
                            Qualifications = model.Qualifications,
                            LicenseNumber = model.LicenseNumber,
                            ExperienceYears = model.ExperienceYears ?? 0
                        };
                        _context.Doctors.Add(doctor);
                    }

                    await _context.SaveChangesAsync();
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return user.Role switch
                    {
                        UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                        UserRole.Doctor => RedirectToAction("Dashboard", "Doctor"),
                        UserRole.Patient => RedirectToAction("Dashboard", "Patient"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            };

            // Load role-specific data
            if (user.Role == UserRole.Patient)
            {
                var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
                if (patient != null)
                {
                    model.DateOfBirth = patient.DateOfBirth;
                    model.Gender = patient.Gender;
                    model.Address = patient.Address;
                    model.EmergencyContact = patient.EmergencyContact;
                    model.BloodGroup = patient.BloodGroup;
                }
            }
            else if (user.Role == UserRole.Doctor)
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                if (doctor != null)
                {
                    model.Specialization = doctor.Specialization;
                    model.Qualifications = doctor.Qualifications;
                    model.LicenseNumber = doctor.LicenseNumber;
                    model.ExperienceYears = doctor.ExperienceYears;
                }
            }

            return View(model);
        }
    }
}