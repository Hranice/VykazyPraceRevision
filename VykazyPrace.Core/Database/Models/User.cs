using System;
using System.Collections.Generic;

namespace VykazyPrace.Core.Database.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public int PersonalNumber { get; set; }

    public string WindowsUsername { get; set; } = null!;

    public int LevelOfAccess { get; set; }

    public int? UserGroupId { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public virtual ICollection<TimeEntrySubType> TimeEntrySubTypes { get; set; } = new List<TimeEntrySubType>();

    public virtual ICollection<ArrivalDeparture> ArrivalDepartures { get; set; } = new List<ArrivalDeparture>();

    public virtual UserGroup? UserGroup { get; set; }
}
