using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class QuizQuestion
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public decimal Points { get; set; }

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizResponse> QuizResponses { get; set; } = new List<QuizResponse>();
}
