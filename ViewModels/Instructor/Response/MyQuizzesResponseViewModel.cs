using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class MyQuizzesResponseViewModel
    {
        public List<Quiz> Quizzes { get; set; } = new();
    }
}
