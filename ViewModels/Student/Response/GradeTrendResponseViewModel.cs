using System.Collections.Generic;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class GradeTrendResponseViewModel
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }
}
