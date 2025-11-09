using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class AcademicTerm
{
    public int AcademicTermId { get; set; }

    public int InstitutionId { get; set; }

    public string TermName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Institution Institution { get; set; } = null!;
}
