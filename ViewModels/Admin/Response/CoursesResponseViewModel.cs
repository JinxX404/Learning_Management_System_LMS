using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class CoursesResponseViewModel
    {
        public List<Course> Courses { get; set; } = new();
        public List<User> Instructors { get; set; } = new();
        public List<AcademicTerm> Terms { get; set; } = new();
        public string? SearchString { get; set; }
        public string? CurrentStatus { get; set; }
        public int? CurrentInstructor { get; set; }
        public int? CurrentTerm { get; set; }
    }
}
