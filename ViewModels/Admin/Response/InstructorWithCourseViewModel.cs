using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class InstructorWithCourseViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Department { get; set; }
        public int? CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseCode { get; set; }
    }
}
