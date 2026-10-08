using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class CourseContentResponseViewModel
    {
        public Course Course { get; set; } = null!;
        public Lecture? Lecture { get; set; }
        public IEnumerable<LearningAsset> Assets { get; set; } = System.Linq.Enumerable.Empty<LearningAsset>();
        public decimal Progress { get; set; }
    }
}
