using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwGradeBookDetail
{
    public int GradeBookId { get; set; }

    public string CourseTitle { get; set; } = null!;

    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string GradableItemType { get; set; } = null!;

    public decimal Points { get; set; }

    public decimal MaxPoints { get; set; }
}
