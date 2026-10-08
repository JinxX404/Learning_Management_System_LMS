using System;
namespace Learning_Management_System.ViewModels.AI.Response
{
    public class AigeneratedQuizViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = null!;
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
    }
}
