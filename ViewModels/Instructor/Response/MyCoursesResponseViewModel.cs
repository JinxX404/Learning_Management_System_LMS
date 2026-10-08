using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class MyCoursesResponseViewModel
    {
        public List<Course> Courses { get; set; } = new();
        public List<AcademicTerm> AcademicTerms { get; set; } = new();
        public string? Error { get; set; }
    }
}
