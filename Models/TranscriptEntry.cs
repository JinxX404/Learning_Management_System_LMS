using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class TranscriptEntry
{
    public int TranscriptEntryId { get; set; }

    public int TranscriptId { get; set; }

    public int CourseId { get; set; }

    public int GradeId { get; set; }

    public string LetterGrade { get; set; } = null!;

    public decimal NumericGrade { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Grade Grade { get; set; } = null!;

    public virtual Transcript Transcript { get; set; } = null!;
}
