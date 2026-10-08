using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class DeleteQuizRequestViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
