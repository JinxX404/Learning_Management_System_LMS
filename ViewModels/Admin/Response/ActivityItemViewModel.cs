using System;

namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class ActivityItemViewModel
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
        public DateTime Date { get; set; }
        public string? Icon { get; set; }
        public string? ColorClass { get; set; }
    }
}