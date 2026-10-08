namespace Learning_Management_System.ViewModels.Instructor.Request
{
    public class OptionViewModel
    {
        public int OptionId { get; set; } = 0; // 0 for new, >0 for existing

        public string OptionText { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
