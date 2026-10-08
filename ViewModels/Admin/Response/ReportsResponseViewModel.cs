using System.Collections.Generic;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class ReportsResponseViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveTerms { get; set; }
        public List<TopPerformingStudentViewModel> TopStudents { get; set; } = new();
        public List<dynamic> CourseRates { get; set; } = new();
        public List<InstitutionSummaryViewModel> InstitutionSummary { get; set; } = new();
    }
}
