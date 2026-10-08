using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class AddLectureRequestViewModel
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
