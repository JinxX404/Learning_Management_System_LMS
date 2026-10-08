using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class CoursesWithEnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public int CurrentEnrollment { get; set; }
        public int MaxEnrollment { get; set; }
    }
}
