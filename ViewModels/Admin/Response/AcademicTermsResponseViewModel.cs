using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class AcademicTermsResponseViewModel
    {
        public List<AcademicTerm> Terms { get; set; } = new();
        public List<Institution> Institutions { get; set; } = new();
    }
}
