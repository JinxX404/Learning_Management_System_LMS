using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class AssignmentsResponseViewModel
    {
        public List<CourseEnrollment> EnrolledCourses { get; set; } = new();
        public int? SelectedCourseId { get; set; }
        public List<dynamic> Assignments { get; set; } = new();
    }
}
