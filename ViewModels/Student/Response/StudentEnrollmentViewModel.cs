using System;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class StudentEnrollmentViewModel
    {
        public int EnrollmentId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
        public string Status { get; set; } = null!;
    }
}
