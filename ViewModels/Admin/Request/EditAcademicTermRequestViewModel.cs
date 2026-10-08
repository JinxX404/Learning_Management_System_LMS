using System.ComponentModel.DataAnnotations;
namespace Learning_Management_System.ViewModels.Admin.Request
{
    public class EditAcademicTermRequestViewModel
    {
        [Required]
        public int AcademicTermId { get; set; }
        [Required]
        public string TermName { get; set; } = null!;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public int InstitutionId { get; set; }
        public bool IsActive { get; set; }
    }
}
