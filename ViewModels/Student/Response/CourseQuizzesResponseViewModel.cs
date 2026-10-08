using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class CourseQuizzesResponseViewModel
    {
        public Course Course { get; set; } = null!;
        public List<QuizListItemViewModel> Quizzes { get; set; } = new();
    }
}
