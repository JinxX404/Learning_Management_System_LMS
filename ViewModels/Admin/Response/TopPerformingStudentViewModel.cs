using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class TopPerformingStudentViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public decimal? Gpa { get; set; }
    }
}
