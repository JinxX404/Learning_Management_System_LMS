using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class CreateAnnouncementRequestViewModel
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string Message { get; set; } = null!;
        public bool IsImportant { get; set; }
    }
}
