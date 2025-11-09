using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class QuizAttempt
{
    public int AttemptId { get; set; }

    public int QuizId { get; set; }

    public int UserId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public decimal? Score { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizResponse> QuizResponses { get; set; } = new List<QuizResponse>();

    public virtual User User { get; set; } = null!;
}
