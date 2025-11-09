using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class QuizResponse
{
    public int ResponseId { get; set; }

    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public string? ResponseText { get; set; }

    public virtual QuizAttempt Attempt { get; set; } = null!;

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual QuestionOption? SelectedOption { get; set; }
}
