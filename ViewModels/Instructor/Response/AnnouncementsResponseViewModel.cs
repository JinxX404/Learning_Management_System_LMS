using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class AnnouncementsResponseViewModel
    {
        public List<Course> Courses { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
    }
}
