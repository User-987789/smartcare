using Microsoft.AspNetCore.Identity;
using SmartCarePatientPortal.Models;

namespace SmartCarePatientPortal.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if we already have data
            if (context.Users.Any())
                return; // Database has been seeded

            // Create sample admin user
            var adminUser = new ApplicationUser
            {
                UserName = "admin@smartcare.com",
                Email = "admin@smartcare.com",
                FirstName = "System",
                LastName = "Administrator",
                Role = UserRole.Admin,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, "Admin@123");

            // Create sample doctor
            var doctorUser = new ApplicationUser
            {
                UserName = "doctor@smartcare.com",
                Email = "doctor@smartcare.com",
                FirstName = "Dr. John",
                LastName = "Smith",
                Role = UserRole.Doctor,
                EmailConfirmed = true
            };

            var doctorResult = await userManager.CreateAsync(doctorUser, "Doctor@123");

            if (doctorResult.Succeeded)
            {
                var doctor = new Doctor
                {
                    UserId = doctorUser.Id,
                    DoctorNumber = "D001",
                    Specialization = "Cardiology",
                    Qualifications = "MBBS, MD Cardiology",
                    LicenseNumber = "MED001",
                    ExperienceYears = 10
                };
                context.Doctors.Add(doctor);
            }

            // Create sample patient
            var patientUser = new ApplicationUser
            {
                UserName = "patient@smartcare.com",
                Email = "patient@smartcare.com",
                FirstName = "Jane",
                LastName = "Doe",
                Role = UserRole.Patient,
                EmailConfirmed = true
            };

            var patientResult = await userManager.CreateAsync(patientUser, "Patient@123");

            if (patientResult.Succeeded)
            {
                var patient = new Patient
                {
                    UserId = patientUser.Id,
                    PatientNumber = "P001",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    Gender = "Female",
                    Address = "123 Main Street, City",
                    EmergencyContact = "555-0123",
                    BloodGroup = "O+"
                };
                context.Patients.Add(patient);
            }

            await context.SaveChangesAsync();
        }
    }
}