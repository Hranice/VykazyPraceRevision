using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Database.Models.OutlookEvents
{
    /// <summary>
    /// Stav položky z pohledu konkrétního uživatele (tombstone apod.).
    /// UNIQUE(UserId, ItemId).
    /// </summary>
    public class UserItemState
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // FK na Users.ID

        [Required]
        public int ItemId { get; set; } // FK na CalendarItems.Id

        [Required]
        public UserItemStateEnum State { get; set; } = UserItemStateEnum.Default;

        public string? Note { get; set; }

        [Required]
        public DateTime UpdatedAtUtc { get; set; }

        public User? User { get; set; }
        public CalendarItem? Item { get; set; }
    }
}
