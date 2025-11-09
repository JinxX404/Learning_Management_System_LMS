using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
