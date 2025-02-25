using System;
using System.Collections.Generic;

namespace VykazyPrace.Core.Database.Models;

public partial class TimeEntry
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ProjectId { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? Description { get; set; }

    public int EntryMinutes { get; set; }

    public virtual Project? Project { get; set; }

    public virtual User? User { get; set; }
}
