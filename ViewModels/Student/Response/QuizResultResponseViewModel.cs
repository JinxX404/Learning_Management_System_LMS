using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class QuizResultResponseViewModel
    {
        public QuizAttempt QuizAttempt { get; set; } = null!;
        public decimal TotalScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
    }
}
