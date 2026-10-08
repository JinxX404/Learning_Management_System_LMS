using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class DeleteLearningAssetRequestViewModel
    {
        [Required]
        public int AssetId { get; set; }
    }
}
