using System;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class QuizListItemViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public int QuestionCount { get; set; }
        public bool IsDeleted { get; set; }
        public string Status { get; set; } = null!;
        public int? LastAttemptId { get; set; }
    }
}
