using System;
using System.Collections.Generic;

namespace VykazyPrace.Core.Database.Models;

public partial class UserGroup
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
