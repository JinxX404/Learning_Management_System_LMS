using System;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class QuizResponseWithCorrectnessViewModel
    {
        public int ResponseId { get; set; }
        public int AttemptId { get; set; }
        public string QuestionText { get; set; } = null!;
        public string? SelectedOption { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal? PointsEarned { get; set; }
    }
}
