using System;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class CourseDetailViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string? Description { get; set; }
        public string InstructorName { get; set; } = null!;
        public string InstructorEmail { get; set; } = null!;
        public string TermName { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string InstitutionName { get; set; } = null!;
    }
}
