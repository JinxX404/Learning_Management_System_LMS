using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class CourseDetailsResponseViewModel
    {
        public Course Course { get; set; } = null!;
        public bool IsEnrolled { get; set; }
    }
}
