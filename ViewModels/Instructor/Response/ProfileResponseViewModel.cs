using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class ProfileResponseViewModel
    {
        public User User { get; set; } = null!;
        public List<Course> Courses { get; set; } = new();
    }
}
