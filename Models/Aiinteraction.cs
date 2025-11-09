using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class Aiinteraction
{
    public int InteractionId { get; set; }

    public int UserId { get; set; }

    public int AimodelId { get; set; }

    public string UserMessage { get; set; } = null!;

    public string? Airesponse { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Aimodel Aimodel { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
