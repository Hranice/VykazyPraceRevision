using System;
using System.Collections.Generic;

namespace VykazyPrace.Core.Database.Models;

public partial class TimeEntryType
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Color { get; set; }

    public int? ForProjectType { get; set; }

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
