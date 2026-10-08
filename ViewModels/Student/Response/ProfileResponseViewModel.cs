using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class ProfileResponseViewModel
    {
        public User User { get; set; } = null!;
        public List<CourseEnrollment> Enrollments { get; set; } = new();
    }
}
