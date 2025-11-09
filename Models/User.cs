using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class User
{
    public int UserId { get; set; }

    public int InstitutionId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<Aiinteraction> Aiinteractions { get; set; } = new List<Aiinteraction>();

    public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Institution Institution { get; set; } = null!;

    public virtual InstructorProfile? InstructorProfile { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual StudentProfile? StudentProfile { get; set; }
}
