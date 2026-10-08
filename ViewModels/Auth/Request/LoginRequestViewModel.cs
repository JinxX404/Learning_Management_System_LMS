using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Auth.Request
{
    public class LoginRequestViewModel
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }
}
