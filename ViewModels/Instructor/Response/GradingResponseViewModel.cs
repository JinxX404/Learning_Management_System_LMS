using System.Collections.Generic;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class GradingResponseViewModel
    {
        public List<dynamic> Grades { get; set; } = new();
        public List<dynamic> InstructorCourses { get; set; } = new();
        public int? SelectedCourseId { get; set; }
    }
}
