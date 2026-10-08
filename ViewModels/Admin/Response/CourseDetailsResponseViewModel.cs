using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class CourseDetailsResponseViewModel
    {
        public Course Course { get; set; } = null!;
        public List<CourseEnrollment> Enrollments { get; set; } = new();
    }
}
