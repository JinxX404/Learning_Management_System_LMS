using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class CreateCourseRequestViewModel
    {
        [Required]
        public string CourseCode { get; set; } = null!;
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public int InstructorId { get; set; }
        [Required]
        public int AcademicTermId { get; set; }
        public int CreditHours { get; set; } = 3;
        public int MaxEnrollment { get; set; } = 50;
    }
}
