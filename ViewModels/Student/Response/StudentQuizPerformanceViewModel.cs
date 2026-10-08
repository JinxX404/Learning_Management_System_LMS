using System;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class StudentQuizPerformanceViewModel
    {
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public int? NumberOfAttempts { get; set; }
        public decimal? AverageScore { get; set; }
    }
}
