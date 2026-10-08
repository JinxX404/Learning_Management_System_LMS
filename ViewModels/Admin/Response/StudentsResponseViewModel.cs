using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class StudentsResponseViewModel
    {
        public List<User> Students { get; set; } = new();
    }
}
