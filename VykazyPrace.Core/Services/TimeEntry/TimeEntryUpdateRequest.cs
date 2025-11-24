using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Services.TimeEntry
{
    /// <summary>
    /// Vstupní model pro hromadnou / single aktualizaci časových záznamů.
    /// </summary>
    public sealed class TimeEntryUpdateRequest
    {
        /// <summary>
        /// Vybraný typ projektu 
        /// (provoz, projekt, ...).
        /// </summary>
        public int CurrentProjectType { get; init; }

        /// <summary>
        /// ID vybraného typu záznamu (práce, administrativa, ...).
        /// </summary>
        /// <summary>
        public int SelectedEntryTypeId { get; init; }

        /// <summary>
        /// ID uživatele, pro kterého se čas vykazuje (kvůli subtypům - indexům).
        /// </summary>
        public int SelectedUserId { get; init; }

        /// <summary>
        /// Text poznámky.
        /// </summary>
        public string? Note { get; init; }

        /// <summary>
        /// Text podtypu (indexu).
        /// </summary>
        public string? SubTypeTitle { get; init; }

        /// <summary>
        /// ID vybraného projektu pro PROVOZ/PROJEKT/PŘEDPROJEKT.
        /// U ostatních typů ignorováno (použijí se fixní projekty 23/25/26).
        /// </summary>
        public int? SelectedProjectId { get; init; }

        /// <summary>
        /// Kolekce ID záznamů pro hromadnou úpravu.
        /// </summary>
        public IReadOnlyCollection<int> SelectedEntryIds { get; init; } = Array.Empty<int>();

        /// <summary>
        /// ID záznamu, který bude použit v případě, že se nejedná o hromadnou úpravou.
        /// </summary>
        public int SelectedTimeEntryId { get; init; }

        /// <summary>
        /// Kolekce aktuálně načtených záznamů.
        /// </summary>
        public IList<VykazyPrace.Core.Database.Models.TimeEntry> CurrentEntries { get; init; } = new List<VykazyPrace.Core.Database.Models.TimeEntry>();

        /// <summary>
        /// Kolekce projektů v aktuálním kontextu (kvůli AfterCare/IsArchived).
        /// </summary>
        public IList<Project> Projects { get; init; } = new List<Project>();
    }
}