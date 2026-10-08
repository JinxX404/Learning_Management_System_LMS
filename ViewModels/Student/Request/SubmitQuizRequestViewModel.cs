using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Student.Request
{
    public class SubmitQuizRequestViewModel
    {
        [Required]
        public int QuizId { get; set; }
        [Required]
        public int AttemptId { get; set; }
        public Dictionary<int, string> Answers { get; set; } = new Dictionary<int, string>();
    }
}
