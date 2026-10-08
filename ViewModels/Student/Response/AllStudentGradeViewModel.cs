using System;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class AllStudentGradeViewModel
    {
        public int GradeId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string GradableItemType { get; set; } = null!;
        public string? ItemTitle { get; set; }
        public decimal Points { get; set; }
        public decimal MaxPoints { get; set; }
        public decimal? Percentage { get; set; }
        public DateTime GradedAt { get; set; }
        public string? Comments { get; set; }
    }
}
