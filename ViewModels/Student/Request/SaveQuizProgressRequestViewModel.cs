using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Student.Request
{
    public class SaveQuizProgressRequestViewModel
    {
        [Required]
        public int AttemptId { get; set; }
        [Required]
        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }
}
