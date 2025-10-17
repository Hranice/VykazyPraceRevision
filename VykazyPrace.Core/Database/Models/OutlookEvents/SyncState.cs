using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models.OutlookEvents
{
    /// <summary>
    /// Per-klientní stav syncu.
    /// </summary>
    public class SyncState
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }
        [MaxLength(128)]
        public string? MachineName { get; set; }

        [Required, MaxLength(128)]
        public string Key { get; set; } = default!;

        public string? Value { get; set; }
    }
}
