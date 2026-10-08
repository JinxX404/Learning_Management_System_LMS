namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class AnnouncementResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int Count { get; set; }
        public bool HasStudents { get; set; }
        public bool SavedToDb { get; set; }
        public int SavedRecords { get; set; }
    }
}
