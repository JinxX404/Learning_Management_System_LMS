using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwAimodelStatus
{
    public string ModelName { get; set; } = null!;

    public bool IsActive { get; set; }
}
