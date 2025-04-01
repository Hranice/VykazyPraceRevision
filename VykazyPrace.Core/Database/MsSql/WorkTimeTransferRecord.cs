namespace VykazyPrace.Core.Database.MsSql
{
    public class WorkTimeTransferRecord
    {
        public int PersonId { get; set; }
        public int PersonalNumber { get; set; }
        public DateTime? Arrival { get; set; }
        public DateTime? Departure { get; set; }
        public string DepartureReason { get; set; }
        public double StandardHours { get; set; }
        public double OvertimeHours { get; set; }
        public DateTime WorkDate { get; set; }
        public string ApprovalState { get; set; }
    }
}
