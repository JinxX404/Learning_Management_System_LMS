using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwCoursesWithInstructorAndTerm
{
    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string InstructorName { get; set; } = null!;

    public string TermName { get; set; } = null!;
}
