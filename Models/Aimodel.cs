using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Aimodel
{
    public int AimodelId { get; set; }

    public string ModelName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<AigeneratedContent> AigeneratedContents { get; set; } = new List<AigeneratedContent>();

    public virtual ICollection<Aiinteraction> Aiinteractions { get; set; } = new List<Aiinteraction>();
}
