namespace VykazyPrace.Core.Services.TimeEntry
{
    /// <summary>
    /// Výsledek hromadné/single aktualizace.
    /// </summary>
    public sealed class TimeEntryUpdateResult
    {
        /// <summary>
        /// Počet záznamů, které se podařilo úspěšně uložit.
        /// </summary>
        public int UpdatedCount { get; init; }

        /// <summary>
        /// True, pokud se podařilo uložit alespoň jeden záznam.
        /// </summary>
        public bool HasAnyUpdate => UpdatedCount > 0;
    }
}
