using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class InstitutionSummaryViewModel
    {
        public int InstitutionId { get; set; }
        public string Name { get; set; } = null!;
        public int? UserCount { get; set; }
        public int? CourseCount { get; set; }
        public int? TermCount { get; set; }
    }
}
