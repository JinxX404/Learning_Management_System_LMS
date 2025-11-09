using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Lecture
{
    public int LectureId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<LearningAsset> LearningAssets { get; set; } = new List<LearningAsset>();
}
