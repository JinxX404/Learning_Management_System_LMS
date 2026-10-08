using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class CreateCourseRequestViewModel
    {
        [Required]
        public string CourseCode { get; set; } = null!;
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int AcademicTermId { get; set; }
        public int CreditHours { get; set; } = 3;
    }
}
