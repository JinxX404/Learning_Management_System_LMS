using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class CreateQuizViewModel
    {
        public int QuizId { get; set; } = 0; // 0 for new quiz, >0 for editing

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public int? TimeLimitMinutes { get; set; }

        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }
}