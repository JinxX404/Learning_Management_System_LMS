using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class CreateInstructorRequestViewModel
    {
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string Status { get; set; } = "Active";
        public string? Department { get; set; }
        public string? Biography { get; set; }
        public int InstitutionId { get; set; }
    }
}
