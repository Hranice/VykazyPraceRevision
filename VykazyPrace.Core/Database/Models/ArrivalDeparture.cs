using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models
{
    public partial class ArrivalDeparture
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime WorkDate { get; set; }

        public DateTime? ArrivalTimestamp { get; set; }

        public DateTime? DepartureTimestamp { get; set; }

        public string? DepartureReason { get; set; }

        public double HoursWorked { get; set; } = 0;

        public double HoursOvertime { get; set; } = 0;

        public virtual User? User { get; set; }
    }
}
