using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class CourseEnrollment
{
    public int EnrollmentId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string Status { get; set; } = null!;

    public decimal? FinalGrade { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
