using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Student.Request
{
    public class MarkLessonCompleteRequestViewModel
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
