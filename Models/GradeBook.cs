using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class GradeBook
{
    public int GradeBookId { get; set; }

    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
