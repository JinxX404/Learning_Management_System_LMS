using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class SendAnnouncementRequestViewModel
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string Message { get; set; } = null!;
    }
}
