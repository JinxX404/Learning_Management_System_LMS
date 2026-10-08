using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class ChangePasswordRequestViewModel
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;
        [Required]
        public string NewPassword { get; set; } = null!;
        [Required]
        public string ConfirmPassword { get; set; } = null!;
    }
}
