using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Preview.Request
{
    public class ViewPageRequestViewModel
    {
        [Required]
        public string Name { get; set; } = null!;
    }
}
