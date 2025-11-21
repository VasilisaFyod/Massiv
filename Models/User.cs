using System;
using System.Collections.Generic;

namespace Massiv.Models;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string Login { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
