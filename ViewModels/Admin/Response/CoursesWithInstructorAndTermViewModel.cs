using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class CoursesWithInstructorAndTermViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string TermName { get; set; } = null!;
    }
}
