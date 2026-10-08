using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class InstructorDetailsResponseViewModel
    {
        public User Instructor { get; set; } = null!;
        public List<Course> Courses { get; set; } = new();
    }
}
