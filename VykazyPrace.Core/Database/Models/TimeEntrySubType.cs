using System;
using System.Collections.Generic;

namespace VykazyPrace.Core.Database.Models;

public partial class TimeEntrySubType
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public int UserId { get; set; }

    public int? IsArchived { get; set; }

    public virtual User User { get; set; } = null!;
}
