using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class CreateCourseResponseViewModel
    {
        public List<User> Instructors { get; set; } = new();
        public List<AcademicTerm> Terms { get; set; } = new();
        public string? Error { get; set; }
    }
}
