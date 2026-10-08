using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class EnrollmentsResponseViewModel
    {
        public List<Course> Courses { get; set; } = new();
        public int? SelectedCourseId { get; set; }
        public List<CourseEnrollment>? Enrollments { get; set; }
        public List<User>? AvailableStudents { get; set; }
    }
}
