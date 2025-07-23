using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models
{
    public class UserTimeReport
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public double ReportedHours { get; set; }
        public double ActualHours { get; set; }
        public double OvertimeHours { get; set; }
        public double MissingHours { get; set; }
    }

}
