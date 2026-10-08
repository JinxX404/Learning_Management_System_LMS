using System;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class GradeBookDetailViewModel
    {
        public int GradeBookId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string GradableItemType { get; set; } = null!;
        public decimal Points { get; set; }
        public decimal MaxPoints { get; set; }
    }
}
