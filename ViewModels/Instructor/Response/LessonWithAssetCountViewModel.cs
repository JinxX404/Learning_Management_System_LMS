using System;
namespace Learning_Management_System.ViewModels.Instructor.Response
{
    public class LessonWithAssetCountViewModel
    {
        public int LectureId { get; set; }
        public string Title { get; set; } = null!;
        public int CourseId { get; set; }
        public int? AssetCount { get; set; }
    }
}
