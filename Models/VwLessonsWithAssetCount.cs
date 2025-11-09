using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwLessonsWithAssetCount
{
    public int LectureId { get; set; }

    public string Title { get; set; } = null!;

    public int CourseId { get; set; }

    public int? AssetCount { get; set; }
}
