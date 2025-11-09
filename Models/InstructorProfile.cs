using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class InstructorProfile
{
    public int InstructorProfileId { get; set; }

    public int UserId { get; set; }

    public string? Department { get; set; }

    public string? Bio { get; set; }

    public virtual User User { get; set; } = null!;
}
