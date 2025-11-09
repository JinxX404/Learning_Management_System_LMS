using Microsoft.AspNetCore.Mvc;

namespace Learning_Management_System.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, bool rememberMe = false)
        {
            // TODO: Implement actual authentication logic
            // For now, redirect to dashboard for any login attempt
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                return RedirectToAction("Dashboard", "Student");
            }
            
            // If login fails, stay on login page
            ViewData["Title"] = "Login";
            ViewBag.ErrorMessage = "Invalid email or password.";
            return View();
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
            // TODO: Implement logout logic
            return RedirectToAction("Login");
        }
    }
}
