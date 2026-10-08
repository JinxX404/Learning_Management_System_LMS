using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class DashboardResponseViewModel
    {
        public int EnrollmentCount { get; set; }
        public List<CourseEnrollment> Enrollments { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
        public int AssignmentCount { get; set; }
    }
}
