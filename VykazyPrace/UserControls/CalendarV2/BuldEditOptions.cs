using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.UserControls.CalendarV2
{
    public sealed class BulkEditOptions
    {
        // Volitelné změny – pokud null, nezměnit
        public int? NewProjectId { get; set; }
        public int? NewEntryTypeId { get; set; }
        public string? NewDescription { get; set; }
        public string? NewNote { get; set; }

        // Přibrat podobné záznamy dle kritérií (volitelné)
        public bool IncludeSimilar { get; set; }

        // Kritéria podobnosti (libovolná kombinace):
        public bool MatchSameProject { get; set; }
        public bool MatchSameEntryType { get; set; }
        public bool MatchSameDescription { get; set; }
        public bool MatchSameNote { get; set; }

        // Rozsah (volitelně) – např. aktuální týden uživatele
        public DateTime? RangeFrom { get; set; }
        public DateTime? RangeTo { get; set; }
    }

}
