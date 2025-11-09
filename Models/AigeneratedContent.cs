using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class AigeneratedContent
{
    public int AicontentId { get; set; }

    public int AimodelId { get; set; }

    public int? CourseId { get; set; }

    public string GeneratedContent { get; set; } = null!;

    public DateTime GeneratedAt { get; set; }

    public virtual Aimodel Aimodel { get; set; } = null!;
}
