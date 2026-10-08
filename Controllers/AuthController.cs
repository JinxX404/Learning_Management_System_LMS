using Microsoft.AspNetCore.Mvc;
using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Learning_Management_System.ViewModels.Auth.Request;
using Learning_Management_System.ViewModels.Auth.Response;
using Microsoft.EntityFrameworkCore;

namespace Learning_Management_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly LmsContext _context;

        public AuthController(LmsContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View(new LoginResponseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return View(new LoginResponseViewModel { ErrorMessage = "Invalid email or password." });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return View(new LoginResponseViewModel { ErrorMessage = "Your account is inactive." });
            }

            // Store user ID in session
            SessionHelper.SetUserId(HttpContext.Session, user.UserId);

            // Update last login time
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Redirect based on role
            if (user.Role == "Student")
                return RedirectToAction("Dashboard", "Student");
            else if (user.Role == "Instructor")
                return RedirectToAction("Dashboard", "Instructor");
            else if (user.Role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Login");
        }

        public IActionResult ResetPassword()
        {
            return View(new ResetPasswordResponseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordRequestViewModel model)
        {
            return View(new ResetPasswordResponseViewModel { SuccessMessage = "If an account with that email exists, we've sent you a password reset link." });
        }

        public IActionResult Logout()
        {
            SessionHelper.ClearSession(HttpContext.Session);
            return RedirectToAction("Login");
        }
    }
}
