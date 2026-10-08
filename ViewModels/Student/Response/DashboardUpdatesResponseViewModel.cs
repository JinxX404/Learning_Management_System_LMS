using System;
using System.Collections.Generic;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class DashboardUpdatesResponseViewModel
    {
        public int UnreadCount { get; set; }
        public List<UpcomingDeadlineItem> UpcomingDeadlines { get; set; } = new();
    }

    public class UpcomingDeadlineItem
    {
        public string Title { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int DueInMinutes { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
