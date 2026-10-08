using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class DeleteLectureRequestViewModel
    {
        [Required]
        public int LectureId { get; set; }
    }
}
