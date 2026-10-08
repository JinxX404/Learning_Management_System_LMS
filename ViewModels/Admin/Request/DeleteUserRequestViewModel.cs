using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class DeleteUserRequestViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
