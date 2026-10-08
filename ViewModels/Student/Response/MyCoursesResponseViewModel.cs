using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class MyCoursesResponseViewModel
    {
        public List<CourseEnrollment> Enrollments { get; set; } = new();
    }
}
