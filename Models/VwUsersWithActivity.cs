using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models;

public partial class VwUsersWithActivity
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }
}
