using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class EditLearningAssetRequestViewModel
    {
        [Required]
        public int AssetId { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string AssetType { get; set; } = null!;
        public string FileUrl { get; set; } = string.Empty;
    }
}
