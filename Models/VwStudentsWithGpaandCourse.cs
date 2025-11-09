using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwStudentsWithGpaandCourse
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public decimal? Gpa { get; set; }

    public int? EnrolledCoursesCount { get; set; }
}
