using System;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class QuizWithStatViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = null!;
        public string CourseTitle { get; set; } = null!;
        public int? QuestionCount { get; set; }
        public int? AttemptCount { get; set; }
    }
}
