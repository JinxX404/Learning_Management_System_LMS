using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Institution
{
    public int InstitutionId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AcademicTerm> AcademicTerms { get; set; } = new List<AcademicTerm>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
