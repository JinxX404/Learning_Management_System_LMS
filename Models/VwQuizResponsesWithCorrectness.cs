using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwQuizResponsesWithCorrectness
{
    public int ResponseId { get; set; }

    public int AttemptId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? SelectedOption { get; set; }

    public bool? IsCorrect { get; set; }

    public decimal? PointsEarned { get; set; }
}
