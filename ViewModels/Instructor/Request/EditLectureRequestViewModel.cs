using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class EditLectureRequestViewModel
    {
        [Required]
        public int LectureId { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
