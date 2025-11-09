using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwCoursePassFailRate
{
    public int CourseId { get; set; }

    public int? PassCount { get; set; }

    public int? FailCount { get; set; }

    public int? TotalCompletedStudents { get; set; }

    public decimal? PassRate { get; set; }
}
