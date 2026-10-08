using System;

namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class ActiveTermViewModel
    {
        public int AcademicTermId { get; set; }

        public string TermName { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public int? CourseCount { get; set; }
    }
}
