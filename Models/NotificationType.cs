using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class NotificationType
{
    public int NotificationTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();
}
