using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwCoursesWithEnrollment
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int CurrentEnrollment { get; set; }

    public int MaxEnrollment { get; set; }
}
