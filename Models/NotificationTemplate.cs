using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class NotificationTemplate
{
    public int TemplateId { get; set; }

    public int NotificationTypeId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string MessageFormat { get; set; } = null!;

    public virtual NotificationType NotificationType { get; set; } = null!;
}
