using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Auth.Request
{
    public class ResetPasswordRequestViewModel
    {
        [Required]
        public string Email { get; set; } = null!;
    }
}
