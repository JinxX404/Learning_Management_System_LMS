using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class AddLearningAssetResponseViewModel
    {
        public Lecture? Lecture { get; set; }
        public string? LectureTitle { get; set; }
        public int? LectureId { get; set; }
        public int CourseId { get; set; }
    }
}
