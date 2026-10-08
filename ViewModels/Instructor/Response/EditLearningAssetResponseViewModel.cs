using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class EditLearningAssetResponseViewModel
    {
        public LearningAsset Asset { get; set; } = null!;
        public string? LectureTitle { get; set; }
        public int LectureId { get; set; }
        public int CourseId { get; set; }
    }
}
