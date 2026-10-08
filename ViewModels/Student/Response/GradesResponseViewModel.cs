using System.Collections.Generic;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class GradesResponseViewModel
    {
        public List<AllStudentGradeViewModel> Grades { get; set; } = new();
    }
}
