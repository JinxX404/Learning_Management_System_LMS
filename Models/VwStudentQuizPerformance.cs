using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwStudentQuizPerformance
{
    public int UserId { get; set; }

    public int QuizId { get; set; }

    public int? NumberOfAttempts { get; set; }

    public decimal? AverageScore { get; set; }
}
