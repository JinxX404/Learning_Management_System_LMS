using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Learning_Management_System.Data
{
    public static class SeedData
    {
        public static async Task SeedAdminUser(LmsContext context, IConfiguration configuration, bool isDevelopment)
        {
            // Never provision a bootstrap admin outside Development.
            if (!isDevelopment)
            {
                return;
            }

            var email = configuration["BootstrapAdmin:Email"];
            var password = configuration["BootstrapAdmin:Password"];

            // No credential configured -> no account is created.
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            // Check if admin already exists
            var adminExists = await context.Users
                .AnyAsync(u => u.Email == email);
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
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var admin = new User
            {
                Email = email,
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
