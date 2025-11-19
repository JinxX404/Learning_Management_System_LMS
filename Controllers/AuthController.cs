using Microsoft.AspNetCore.Mvc;
using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
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
            ViewData["Title"] = "Login";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            // Check if user exists and password is correct
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                ViewData["Title"] = "Login";
                return View();
            }

            // Check if user is active
            if (!user.IsActive)
            {
                ViewBag.ErrorMessage = "Your account is inactive.";
                ViewData["Title"] = "Login";
                return View();
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
            ViewData["Title"] = "Reset Password";
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email)
        {
            // TODO: Implement password reset logic
            ViewBag.SuccessMessage = "If an account with that email exists, we've sent you a password reset link.";
            ViewData["Title"] = "Reset Password";
            return View();
        }

        public IActionResult Logout()
        {
            SessionHelper.ClearSession(HttpContext.Session);
            return RedirectToAction("Login");
        }
    }
}
