using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwAigeneratedQuiz
{
    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = null!;

    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;
}
