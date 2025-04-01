using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VykazyPrace.Core.Database.Models
{
    [Table("WorkTimeTransfers")]
    public class WorkTimeTransfer
    {
        [Key]
        public int Id { get; set; }

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
