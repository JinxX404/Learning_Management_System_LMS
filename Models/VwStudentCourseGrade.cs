using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwStudentCourseGrade
{
    public int GradeId { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = null!;

    public string GradableItemType { get; set; } = null!;

    public string? ItemTitle { get; set; }

    public decimal Points { get; set; }

    public decimal MaxPoints { get; set; }

    public decimal? Percentage { get; set; }

    public DateTime GradedAt { get; set; }

    public string? Comments { get; set; }
}
