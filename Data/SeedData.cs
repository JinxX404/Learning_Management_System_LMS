using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Learning_Management_System.Data
{
    public static class SeedData
    {
        public static async Task SeedAdminUser(LmsContext context)
        {
            // Check if admin already exists
            var adminExists = await context.Users
                .AnyAsync(u => u.Email == "admin@lms.com");
            if (adminExists) return;

            // Get or create institution
            var institution = await context.Institutions.FirstOrDefaultAsync();
            if (institution == null)
            {
                institution = new Institution
                {
                    Name = "Default Institution",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Institutions.Add(institution);
                await context.SaveChangesAsync();
            }

            // Create admin user
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            var admin = new User
            {
                Email = "admin@lms.com",
                PasswordHash = hashedPassword,
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                InstitutionId = institution.InstitutionId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}

