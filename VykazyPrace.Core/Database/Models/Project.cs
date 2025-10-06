using System;
using System.Collections.Generic;

namespace VykazyPrace.Core.Database.Models;

public partial class Project
{
    public int Id { get; set; }

    public int ProjectType { get; set; }

    public string ProjectTitle { get; set; } = null!;

    public string ProjectDescription { get; set; } = null!;

    public int CreatedBy { get; set; }

    public string? Note { get; set; }

    public int IsArchived { get; set; }

    public DateTime? DateFullFilled { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
