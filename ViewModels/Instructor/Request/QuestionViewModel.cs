using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.ViewModels.Instructor.Request
{
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
}