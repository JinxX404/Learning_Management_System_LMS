using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModels
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

    public class QuestionViewModel
    {
        public int QuestionId { get; set; } = 0; // 0 for new, >0 for existing

        [Required]
        public string QuestionText { get; set; } = null!;

        [Required]
        public string QuestionType { get; set; } = "MultipleChoice"; // MultipleChoice, TrueFalse, ShortAnswer

        public decimal Points { get; set; } = 1;

        public List<OptionViewModel> Options { get; set; } = new List<OptionViewModel>();

        // For Short Answer questions, or to hold the correct answer text for simple validation
        public string? CorrectAnswerText { get; set; }
    }

    public class OptionViewModel
    {
        public int OptionId { get; set; } = 0; // 0 for new, >0 for existing
        
        public string OptionText { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
