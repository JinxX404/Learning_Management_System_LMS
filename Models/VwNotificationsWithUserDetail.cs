using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwNotificationsWithUserDetail
{
    public int NotificationId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;
}
