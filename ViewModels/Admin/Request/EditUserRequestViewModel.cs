using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class EditUserRequestViewModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        public string Role { get; set; } = null!;
        public string? Password { get; set; }
        public bool IsActive { get; set; }
    }
}
