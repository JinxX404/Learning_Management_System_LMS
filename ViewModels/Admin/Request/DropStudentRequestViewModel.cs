using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class DropStudentRequestViewModel
    {
        [Required]
        public int EnrollmentId { get; set; }
    }
}
