using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class StudentProfile
{
    public int StudentProfileId { get; set; }

    public int UserId { get; set; }

    public string? StudentIdNumber { get; set; }

    public DateOnly? AdmissionDate { get; set; }

    public decimal? Gpa { get; set; }

    public virtual ICollection<Transcript> Transcripts { get; set; } = new List<Transcript>();

    public virtual User User { get; set; } = null!;
}
