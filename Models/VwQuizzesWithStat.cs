using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwQuizzesWithStat
{
    public int QuizId { get; set; }

    public string Title { get; set; } = null!;

    public string CourseTitle { get; set; } = null!;

    public int? QuestionCount { get; set; }

    public int? AttemptCount { get; set; }
}
