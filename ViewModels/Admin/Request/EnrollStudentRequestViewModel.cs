using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class EnrollStudentRequestViewModel
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int StudentId { get; set; }
    }
}
