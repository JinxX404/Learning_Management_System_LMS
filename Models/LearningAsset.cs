using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class LearningAsset
{
    public int AssetId { get; set; }

    public int LectureId { get; set; }

    public string Title { get; set; } = null!;

    public string AssetType { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public virtual Lecture Lecture { get; set; } = null!;
}
