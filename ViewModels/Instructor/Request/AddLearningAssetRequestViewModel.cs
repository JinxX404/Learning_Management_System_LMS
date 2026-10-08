using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class AddLearningAssetRequestViewModel
    {
        public int? CourseId { get; set; }
        public int? LectureId { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string AssetType { get; set; } = null!;
        public string FileUrl { get; set; } = string.Empty;
    }
}
