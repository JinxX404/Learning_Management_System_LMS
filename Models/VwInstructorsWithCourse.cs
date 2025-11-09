using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwInstructorsWithCourse
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Department { get; set; }

    public int? CourseId { get; set; }

    public string? CourseTitle { get; set; }

    public string? CourseCode { get; set; }
}
