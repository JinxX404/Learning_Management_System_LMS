using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class CreateInstructorResponseViewModel
    {
        public List<Institution> Institutions { get; set; } = new();
        public string? Error { get; set; }
    }
}
