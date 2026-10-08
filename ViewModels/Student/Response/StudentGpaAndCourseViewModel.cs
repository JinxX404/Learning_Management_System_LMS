using System;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class StudentGpaAndCourseViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal? Gpa { get; set; }
        public int? EnrolledCoursesCount { get; set; }
    }
}
