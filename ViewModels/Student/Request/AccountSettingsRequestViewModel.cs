using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Student.Request
{
    public class AccountSettingsRequestViewModel
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
    }
}
