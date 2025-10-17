using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;

namespace VykazyPrace.Core.Services
{
    /// <summary>
    /// Výchozí mapování „meeting z kalendáře“ → TimeEntry.
    /// Uprav si ID projektu/typu podle své databáze.
    /// </summary>
    public static class CalendarImportDefaults
    {
        // TODO: nahraď svými ID (viz tvůj příklad u radioButton3)
        public const int ProjectIdForMeetings = 26; // např. „Porady/Meetingy“
        public const int EntryTypeIdForMeetings = 16; // např. „Schůzka“
        public const int MinimumMinutes = 30;   // zaokrouhlení
        public const int RoundStepMinutes = 30; // 30min sloty
        public const int AllDayDefaultMinutes = 480; // 8h
    }

    /// <summary>
    /// Logika vytvoření TimeEntry z kalendářové položky (CalendarItem).
    /// </summary>
    public class CalendarImportService
    {
        private readonly TimeEntryRepository _timeRepo;

        public CalendarImportService() => _timeRepo = new TimeEntryRepository();

        /// <summary>
        /// Vytvoří TimeEntry pro uživatele na základě CalendarItem.
        /// </summary>
        /// <param name="userId">Uživatel</param>
        /// <param name="item">Zdrojová kalendářová položka</param>
        /// <param name="subject">Předmět pro popis</param>
        /// <param name="startLocal">Začátek v lokálním čase (může být null pro celodenní)</param>
        /// <param name="endLocal">Konec v lokálním čase (může být null)</param>
        /// <returns>Vytvořený záznam nebo null, pokud se rozhodneme nic nevytvářet.</returns>
        public async Task<TimeEntry?> CreateTimeEntryForMeetingAsync(
            int userId,
            CalendarItem item,
            string subject,
            DateTime? startLocal,
            DateTime? endLocal)
        {
            // 1) Určení času a délky
            DateTime timestamp; // uložíme začátek (lokálně), aby seděl s tvými dotazy nad Date
            int minutes;

            if (startLocal.HasValue && endLocal.HasValue && endLocal > startLocal)
            {
                timestamp = startLocal.Value;
                var dur = endLocal.Value - startLocal.Value;

                // Zaokrouhlení na 30 min sloty, min. 30 min
                minutes = RoundToStep((int)Math.Round(dur.TotalMinutes),
                                      CalendarImportDefaults.RoundStepMinutes);
                if (minutes < CalendarImportDefaults.MinimumMinutes)
                    minutes = CalendarImportDefaults.MinimumMinutes;
            }
            else
            {
                // Celodenní nebo bez konce → default 8h
                timestamp = (startLocal ?? DateTime.Now.Date);
                minutes = CalendarImportDefaults.AllDayDefaultMinutes;
            }

            // 2) Vyplnit entity
            var te = new TimeEntry
            {
                UserId = userId,
                ProjectId = CalendarImportDefaults.ProjectIdForMeetings,
                EntryTypeId = CalendarImportDefaults.EntryTypeIdForMeetings,
                Timestamp = timestamp,               // DATETIME lokálně – tvoje repo počítá přes .Date
                Description = subject,               // popis = předmět meetingu
                EntryMinutes = minutes,
                AfterCare = 0,
                Note = $"Outlook import (ItemId={item.Id})",
                IsLocked = 0,
                IsValid = 1
            };

            // 3) Prevence duplicit:
            //    - pokud už pro ten den, projekt a typ existuje záznam se stejným popisem a velmi podobným časem,
            //      duplicitní vložení přeskočíme. (Můžeš zpřísnit/změkčit dle potřeby.)
            var sameDayExists = await _timeRepo.ExistsEntryAsync(
                userId,
                te.Timestamp!.Value.Date,
                te.ProjectId!.Value,
                te.EntryTypeId!.Value
            );

            if (sameDayExists)
            {
                // jemnější check – pokus o nalezení úplně stejného popisu a minuty
                var todays = await _timeRepo.GetTimeEntriesByUserAndDateAsync(
                    new User { Id = userId }, te.Timestamp!.Value.Date);

                foreach (var e in todays)
                {
                    if (e.ProjectId == te.ProjectId
                        && e.EntryTypeId == te.EntryTypeId
                        && e.Description?.Trim() == te.Description?.Trim()
                        && Math.Abs((e.Timestamp!.Value - te.Timestamp!.Value).TotalMinutes) <= 5)
                    {
                        // považuj za duplicitní; nic nevytvářet
                        return null;
                    }
                }
            }

            // 4) Vytvoření
            return await _timeRepo.CreateTimeEntryAsync(te);
        }

        private static int RoundToStep(int minutes, int step)
        {
            if (step <= 0) return minutes;
            var rounded = (int)Math.Round(minutes / (double)step) * step;
            return rounded;
        }
    }
}
