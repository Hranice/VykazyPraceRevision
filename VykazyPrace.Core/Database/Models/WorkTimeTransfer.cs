using System;
using System.Collections.Generic;

namespace VykazyPrace.Core.Database.Models;

public partial class WorkTimeTransfer
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public int PersonalNumber { get; set; }

    public string? Arrival { get; set; }

    public string? Departure { get; set; }

    public string? DepartureReason { get; set; }

    public double StandardHours { get; set; }

    public double OvertimeHours { get; set; }

    public string WorkDate { get; set; } = null!;

    public string? ApprovalState { get; set; }
}
