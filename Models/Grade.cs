using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Grade
{
    public int GradeId { get; set; }

    public int GradeBookId { get; set; }

    public int UserId { get; set; }

    public string GradableItemType { get; set; } = null!;

    public int GradableItemId { get; set; }

    public decimal Points { get; set; }

    public decimal MaxPoints { get; set; }

    public decimal? Percentage { get; set; }

    public string? Comments { get; set; }

    public DateTime GradedAt { get; set; }

    public virtual GradeBook GradeBook { get; set; } = null!;

    public virtual ICollection<TranscriptEntry> TranscriptEntries { get; set; } = new List<TranscriptEntry>();

    public virtual User User { get; set; } = null!;
}
