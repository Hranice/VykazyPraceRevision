using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Core.Services.TimeEntry
{
    /// <summary>
    /// Service, který řeší vše kolem aktualizace TimeEntry
    /// </summary>
    public sealed class TimeEntryUpdateService
    {
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepo;

        public TimeEntryUpdateService(
            TimeEntryRepository timeEntryRepo,
            TimeEntrySubTypeRepository timeEntrySubTypeRepo)
        {
            _timeEntryRepo = timeEntryRepo;
            _timeEntrySubTypeRepo = timeEntrySubTypeRepo;
        }

        /// <summary>
        /// Na základě request.CurrentProjectType rozhodne, jaké ProjectId a Description použít,
        /// případně vytvoří subtyp a následně provede hromadnou / single aktualizaci.
        /// </summary>
        public async Task<TimeEntryUpdateResult> UpdateEntriesAsync(TimeEntryUpdateRequest request)
        {
            if (request.CurrentEntries is null || request.CurrentEntries.Count == 0)
            {
                AppLogger.Error("[EntryUpdate]: Nebyly předány žádné TimeEntry k aktualizaci.");
                return new TimeEntryUpdateResult { UpdatedCount = 0 };
            }

            int projectId;
            string? description = null;

            switch (request.CurrentProjectType)
            {
                // PROVOZ / PROJEKT / PŘEDPROJEKT
                case 0:
                case 1:
                case 2:
                    if (!request.SelectedProjectId.HasValue)
                    {
                        AppLogger.Error("[EntryUpdate]: Pro typ projektu 0/1/2 chybí SelectedProjectId.");
                        return new TimeEntryUpdateResult { UpdatedCount = 0 };
                    }
                    projectId = request.SelectedProjectId.Value;
                    description = await CreateSubTypeIfNeededAsync(request);
                    break;

                // ŠKOLENÍ – projektId 26, bez description
                case 3:
                    projectId = 26;
                    description = null;
                    break;

                // NEPŘÍTOMNOST – projektId 23, bez description
                case 4:
                    projectId = 23;
                    description = null;
                    break;

                // OSTATNÍ – projektId 25, s optional subtypem
                case 5:
                default:
                    projectId = 25;
                    description = await CreateSubTypeIfNeededAsync(request);
                    break;
            }

            int updatedCount = await UpdateEntriesCoreAsync(
                request.CurrentEntries,
                request.SelectedEntryIds,
                request.SelectedTimeEntryId,
                description,
                request.Note,
                request.SelectedEntryTypeId,
                projectId,
                request.Projects
            );

            if (updatedCount > 1)
            {
                AppLogger.Information($"[EntryUpdate]: Hromadná úprava dokončena ({updatedCount} záznamů aktualizováno).");
            }

            return new TimeEntryUpdateResult { UpdatedCount = updatedCount };
        }

        /// <summary>
        /// Vytvoří nový TimeEntrySubType, pokud splňuje podmínky.
        /// Vrací Title vytvořeného subtypu, nebo null.
        /// </summary>
        private async Task<string?> CreateSubTypeIfNeededAsync(TimeEntryUpdateRequest request)
        {
            // Školení/absence subtyp nepoužívají.
            if (request.CurrentProjectType is 3 or 4)
                return null;

            if (string.IsNullOrWhiteSpace(request.SubTypeTitle))
                return null;

            var newSubType = new TimeEntrySubType
            {
                Title = request.SubTypeTitle.Trim(),
                UserId = request.SelectedUserId
            };

            var addedSubType = await _timeEntrySubTypeRepo.CreateTimeEntrySubTypeAsync(newSubType);
            return addedSubType?.Title;
        }

        /// <summary>
        /// Single/hromadná aktualizace TimeEntry.
        /// </summary>
        private async Task<int> UpdateEntriesCoreAsync(
            IList<VykazyPrace.Core.Database.Models.TimeEntry> currentEntries,
            IReadOnlyCollection<int> selectedEntryIds,
            int selectedTimeEntryId,
            string? description,
            string? note,
            int selectedEntryTypeId,
            int selectedProjectId,
            IList<Project> projects)
        {
            var entriesToUpdate = new List<VykazyPrace.Core.Database.Models.TimeEntry>();

            if (selectedEntryIds != null && selectedEntryIds.Count > 0)
            {
                // hromadná úprava
                entriesToUpdate.AddRange(
                    currentEntries.Where(e => selectedEntryIds.Contains(e.Id))
                );
            }
            else
            {
                // single úprava
                var entry = currentEntries.FirstOrDefault(e => e.Id == selectedTimeEntryId);
                if (entry == null)
                {
                    AppLogger.Error($"[EntryUpdate]: Nelze najít záznam s Id = {selectedTimeEntryId}.");
                    return 0;
                }
                entriesToUpdate.Add(entry);
            }

            if (!entriesToUpdate.Any())
            {
                AppLogger.Information("[EntryUpdate]: Nejsou žádné záznamy k aktualizaci (výběr je prázdný).");
                return 0;
            }

            int afterCare = projects
                .FirstOrDefault(p => p.Id == selectedProjectId)?.IsArchived ?? 0;

            int updatedCount = 0;

            foreach (var entry in entriesToUpdate)
            {
                entry.Description = description;
                entry.Note = note;
                entry.EntryTypeId = selectedEntryTypeId;
                entry.ProjectId = selectedProjectId;
                entry.AfterCare = afterCare;
                entry.IsValid = 1;

                bool success = await _timeEntryRepo.UpdateTimeEntryAsync(entry);
                if (success)
                {
                    AppLogger.Information(
                        $"[EntryUpdate({entry.Id})]: Záznam {FormatHelper.FormatTimeEntryToString(entry)} byl úspěšně aktualizován.");
                    updatedCount++;
                }
                else
                {
                    AppLogger.Error(
                        $"[EntryUpdate({entry.Id})]: Záznam {FormatHelper.FormatTimeEntryToString(entry)} se nepodařilo aktualizovat.");
                }
            }

            return updatedCount;
        }
    }
}