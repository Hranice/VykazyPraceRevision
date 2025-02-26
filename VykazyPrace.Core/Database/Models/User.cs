﻿using System;
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

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
