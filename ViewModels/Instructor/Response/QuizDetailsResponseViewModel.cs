using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class QuizDetailsResponseViewModel
    {
        public Quiz Quiz { get; set; } = null!;
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }
}
