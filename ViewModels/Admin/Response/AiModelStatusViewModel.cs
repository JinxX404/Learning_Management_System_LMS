using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class AiModelStatusViewModel
    {
        public string ModelName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
