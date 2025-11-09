using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwActiveTermsWithCourseCount
{
    public int AcademicTermId { get; set; }

    public string TermName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int? CourseCount { get; set; }
}
