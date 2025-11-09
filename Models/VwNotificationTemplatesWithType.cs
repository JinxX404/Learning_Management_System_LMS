using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwNotificationTemplatesWithType
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string MessageFormat { get; set; } = null!;

    public string NotificationType { get; set; } = null!;
}
