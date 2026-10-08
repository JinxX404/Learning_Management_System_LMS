using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class CourseRosterResponseViewModel
    {
        public Course Course { get; set; } = null!;
        public List<CourseEnrollment> Enrollments { get; set; } = new();
    }
}
