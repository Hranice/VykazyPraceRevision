using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models.OutlookEvents
{
    /// <summary>
    /// Auditní log změn (sync i akce uživatele).
    /// </summary>
    public class ItemChangeLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }

        public int? UserId { get; set; } // null = systémový záznam (sync)

        [Required]
        public DateTime WhenUtc { get; set; }

        [Required, MaxLength(64)]
        public string Action { get; set; } = default!; // např. SYNC_UPSERT, USER_SET_STATE, SYNC_MOVED, SYNC_CHANGED...

        public string? DetailsJson { get; set; }

        public CalendarItem? Item { get; set; }
        public User? User { get; set; }
    }
}
