using System;
using System.Collections.Generic;

namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class DashboardResponseViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int CourseEnrollments { get; set; }
        public int AiGenerations { get; set; }
        public int NewUsersLastMonth { get; set; }
        public int NewEnrollmentsLastMonth { get; set; }
        public List<ActivityItemViewModel> RecentActivity { get; set; } = new();
        public List<AiUsageItem> AiUsage { get; set; } = new();
    }

    public class AiUsageItem
    {
        public string Model { get; set; } = null!;
        public int Count { get; set; }
    }
}
