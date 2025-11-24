using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models.OutlookEvents
{
    /// <summary>
    /// Účastníci položky – pro mapování, že dva uživatelé jsou na stejné schůzce.
    /// </summary>
    public class ItemAttendee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }

        [MaxLength(512)]
        public string? DisplayName { get; set; }

        [MaxLength(512)]
        public string? Email { get; set; }

        public int? UserId { get; set; } // TODO: dohledání podle emailu

        [MaxLength(32)]
        public string? Role { get; set; } // Required/Optional/Organizer...

        [MaxLength(32)]
        public string? ResponseStatus { get; set; } // Accepted/Tentative/Declined/None...

        // Navigace
        public CalendarItem? Item { get; set; }
        public User? User { get; set; }
    }
}
