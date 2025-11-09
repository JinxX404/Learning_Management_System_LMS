using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Transcript
{
    public int TranscriptId { get; set; }

    public int StudentProfileId { get; set; }

    public DateTime GeneratedAt { get; set; }

    public decimal? CumulativeGpa { get; set; }

    public int? TotalCredits { get; set; }

    public virtual StudentProfile StudentProfile { get; set; } = null!;

    public virtual ICollection<TranscriptEntry> TranscriptEntries { get; set; } = new List<TranscriptEntry>();
}
