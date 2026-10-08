using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Student.Response
{
    public class QuizPageResponseViewModel
    {
        public Quiz Quiz { get; set; } = null!;
        public int? AttemptId { get; set; }
        public DateTime? StartedAt { get; set; }
        public Dictionary<int, string>? ExistingResponses { get; set; }
    }
}
