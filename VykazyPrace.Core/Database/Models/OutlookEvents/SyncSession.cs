using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models.OutlookEvents
{
    /// <summary>
    /// Souhrnné logy synchronizací
    /// </summary>
    public class SyncSession
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }
        [MaxLength(128)]
        public string? MachineName { get; set; }

        [Required]
        public DateTime StartedUtc { get; set; }
        public DateTime? FinishedUtc { get; set; }

        public int ItemsScanned { get; set; }
        public int ItemsUpserted { get; set; }
        public int ItemsChanged { get; set; }
        public int ItemsMoved { get; set; }
        public int ItemsDeleted { get; set; }
    }
}
