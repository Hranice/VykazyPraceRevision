using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models.OutlookEvents
{
    /// <summary>
    /// Hlavní tabulka kalendářových položek.
    /// </summary>
    public class CalendarItem
    {
        [Key]
        public int Id { get; set; }

        // Identita z Outlooku (OOM)
        [Required, MaxLength(512)]
        public string StoreId { get; set; } = default!;

        [Required, MaxLength(1024)]
        public string EntryId { get; set; } = default!;

        /// <summary>
        /// Začátek instance v UTC pro rozlišení výskytu opakované schůzky.
        /// Pro ne-opakující se je NULL.
        /// </summary>
        public DateTime? OccurrenceStartUtc { get; set; }

        // Stabilnější identifikátory napříč kopiemi / Graphem
        [MaxLength(1024)]
        public string? GlobalAppointmentId { get; set; }

        [MaxLength(1024)]
        public string? ICalUid { get; set; }

        // Poslední "otisk" a metadata
        [Required]
        public DateTime LastSeenAtUtc { get; set; }

        public DateTime? LastModifiedUtc { get; set; }

        [MaxLength(1024)]
        public string? LastFolderEntryId { get; set; }

        [MaxLength(200)]
        public string? LastHash { get; set; } // např. hash(Subject|Start|End|Location|Busy|Folder)

        // Poslední známá "uživatelská" metadata – pro rychlé zobrazení
        [MaxLength(512)]
        public string? Subject { get; set; }

        [MaxLength(512)]
        public string? Location { get; set; }

        [MaxLength(512)]
        public string? Organizer { get; set; }

        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }

        public bool IsAllDay { get; set; }
        public bool IsRecurringSeries { get; set; }  // 1 = master série
        public bool IsException { get; set; }        // 1 = výjimka v sérii

        // Navigace
        public ICollection<UserItemState> UserStates { get; set; } = new List<UserItemState>();
        public ICollection<ItemChangeLog> ChangeLogs { get; set; } = new List<ItemChangeLog>();
        public ICollection<ItemAttendee> Attendees { get; set; } = new List<ItemAttendee>();
    }
}
