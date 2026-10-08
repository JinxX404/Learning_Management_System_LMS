using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class SettingsResponseViewModel
    {
        public User User { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
