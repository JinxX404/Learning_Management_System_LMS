using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwInstitutionSummary
{
    public int InstitutionId { get; set; }

    public string Name { get; set; } = null!;

    public int? UserCount { get; set; }

    public int? CourseCount { get; set; }

    public int? TermCount { get; set; }
}
