using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwAiinteractionsByUser
{
    public int InteractionId { get; set; }

    public string UserMessage { get; set; } = null!;

    public string? Airesponse { get; set; }

    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string ModelName { get; set; } = null!;
}
