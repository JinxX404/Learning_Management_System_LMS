using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class CoursePassFailRateViewModel
    {
        public int CourseId { get; set; }
        public int? PassCount { get; set; }
        public int? FailCount { get; set; }
        public int? TotalCompletedStudents { get; set; }
        public decimal? PassRate { get; set; }
    }
}
