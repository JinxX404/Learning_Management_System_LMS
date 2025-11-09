using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public int InstructorId { get; set; }

    public int AcademicTermId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CreditHours { get; set; }

    public int MaxEnrollment { get; set; }

    public int CurrentEnrollment { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AcademicTerm AcademicTerm { get; set; } = null!;

    public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();

    public virtual GradeBook? GradeBook { get; set; }

    public virtual User Instructor { get; set; } = null!;

    public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<TranscriptEntry> TranscriptEntries { get; set; } = new List<TranscriptEntry>();
}
