using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Import
{
    public sealed class ExternalTimeEntryImportService
    {
        private const int FallbackProjectId = 31;
        private const int SnackOrSpecialEntryTypeId = 1;
        private const int NormalEntryTypeId = 10;

        public async Task<ExternalImportResult> ImportAsync(string filePath)
        {
            var result = new ExternalImportResult();

            using var context = new VykazyPraceContext();
            using var workbook = new XLWorkbook(filePath);

            var users = await context.Users.AsNoTracking().ToListAsync();
            var projects = await context.Projects.AsNoTracking().ToListAsync();

            var entriesToImport = new List<TimeEntry>();
            var affectedUserDays = new HashSet<(int UserId, DateTime Day)>();

            foreach (var worksheet in workbook.Worksheets)
            {
                var sheetName = worksheet.Name.Trim();

                if (!IsExpectedTimeSheet(worksheet))
                {
                    result.Warnings.Add($"List '{sheetName}' přeskočen – nemá očekávanou hlavičku.");
                    continue;
                }

                var user = ResolveUserBySheetName(users, sheetName);
                if (user == null)
                {
                    result.Errors.Add($"List '{sheetName}': uživatel nebyl nalezen.");
                    continue;
                }

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                var dayMinutes = new Dictionary<DateTime, int>();

                for (var rowNumber = 3; rowNumber <= lastRow; rowNumber++)
                {
                    var row = worksheet.Row(rowNumber);

                    var dateCell = row.Cell(1);
                    var projectNumberCell = row.Cell(2);
                    var projectNameCell = row.Cell(3);
                    var topicCell = row.Cell(4);
                    var noteCell = row.Cell(5);
                    var hoursCell = row.Cell(6);

                    var hasAnyImportValue =
                        !IsCellEmpty(dateCell) ||
                        !IsCellEmpty(projectNumberCell) ||
                        !IsCellEmpty(projectNameCell) ||
                        !IsCellEmpty(topicCell) ||
                        !IsCellEmpty(noteCell) ||
                        !IsCellEmpty(hoursCell);

                    if (!hasAnyImportValue)
                        continue;

                    var hasRequiredValues =
                        !IsCellEmpty(dateCell) &&
                        !IsCellEmpty(hoursCell);

                    if (!hasRequiredValues)
                    {
                        result.Warnings.Add(
                            $"List '{sheetName}', řádek {rowNumber}: přeskočeno – chybí datum nebo hodiny.");
                        continue;
                    }

                    var projectNumber = projectNumberCell.GetString().Trim();
                    var projectName = projectNameCell.GetString().Trim();
                    var topic = topicCell.GetString().Trim();
                    var note = noteCell.GetString().Trim();

                    if (!TryReadDate(dateCell, out var date))
                    {
                        result.Errors.Add($"List '{sheetName}', řádek {rowNumber}: neplatné datum.");
                        continue;
                    }

                    if (!TryReadHours(hoursCell, out var hours))
                    {
                        result.Errors.Add($"List '{sheetName}', řádek {rowNumber}: neplatná hodnota hodin.");
                        continue;
                    }

                    var entryMinutes = (int)Math.Round(hours * 60, MidpointRounding.AwayFromZero);

                    if (entryMinutes <= 0)
                    {
                        result.Errors.Add(
                            $"List '{sheetName}', řádek {rowNumber}: počet minut musí být větší než 0.");
                        continue;
                    }

                    var day = date.Date;

                    if (!dayMinutes.ContainsKey(day))
                        dayMinutes[day] = 0;

                    var timestamp = day
                        .AddHours(9)
                        .AddMinutes(30)
                        .AddMinutes(dayMinutes[day]);

                    dayMinutes[day] += entryMinutes;

                    var project = projects.FirstOrDefault(p =>
                        string.Equals(
                            p.ProjectDescription?.Trim(),
                            projectNumber,
                            StringComparison.OrdinalIgnoreCase));

                    var projectId = project?.Id ?? FallbackProjectId;

                    if (project == null)
                    {
                        var projectInfo = string.IsNullOrWhiteSpace(projectName)
                            ? projectNumber
                            : $"{projectNumber} / {projectName}";

                        result.Warnings.Add(
                            $"List '{sheetName}', řádek {rowNumber}: projekt '{projectInfo}' nenalezen, použit ProjectId = {FallbackProjectId}.");
                    }

                    var entryTypeId = projectNumber == "999"
                        ? SnackOrSpecialEntryTypeId
                        : NormalEntryTypeId;

                    var entry = new TimeEntry
                    {
                        UserId = user.Id,
                        ProjectId = projectId,
                        EntryTypeId = entryTypeId,
                        Timestamp = timestamp,
                        Description = topic,
                        Note = note,
                        EntryMinutes = entryMinutes,
                        AfterCare = 0,
                        IsLocked = 0,
                        IsValid = 1
                    };

                    entriesToImport.Add(entry);
                    affectedUserDays.Add((user.Id, day));
                }
            }

            if (result.Errors.Count > 0)
                return result;

            if (entriesToImport.Count == 0)
            {
                result.Errors.Add("V souboru nebyly nalezeny žádné záznamy k importu.");
                return result;
            }

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                foreach (var group in affectedUserDays.GroupBy(x => x.UserId))
                {
                    var userId = group.Key;
                    var days = group.Select(x => x.Day).Distinct().ToList();

                    foreach (var day in days)
                    {
                        var nextDay = day.AddDays(1);

                        var oldEntries = await context.TimeEntries
                            .Where(te =>
                                te.UserId == userId &&
                                te.Timestamp.HasValue &&
                                te.Timestamp.Value >= day &&
                                te.Timestamp.Value < nextDay)
                            .ToListAsync();

                        context.TimeEntries.RemoveRange(oldEntries);
                        result.DeletedCount += oldEntries.Count;
                    }
                }

                context.TimeEntries.AddRange(entriesToImport);
                result.ImportedCount = entriesToImport.Count;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Success = true;
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static bool IsExpectedTimeSheet(IXLWorksheet worksheet)
        {
            return HeaderEquals(worksheet.Cell(2, 1), "Datum")
                && HeaderEquals(worksheet.Cell(2, 2), "Číslo projektu")
                && HeaderEquals(worksheet.Cell(2, 3), "Název projektu")
                && HeaderEquals(worksheet.Cell(2, 4), "Topic")
                && HeaderEquals(worksheet.Cell(2, 5), "Popis práce")
                && HeaderEquals(worksheet.Cell(2, 6), "Hodiny");
        }

        private static bool HeaderEquals(IXLCell cell, string expected)
        {
            return string.Equals(
                cell.GetString().Trim(),
                expected,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCellEmpty(IXLCell cell)
        {
            if (cell.IsEmpty())
                return true;

            return string.IsNullOrWhiteSpace(cell.GetString());
        }

        private static User? ResolveUserBySheetName(List<User> users, string sheetName)
        {
            var normalizedSheetName = Normalize(sheetName);

            return users.FirstOrDefault(u =>
                Normalize($"{u.FirstName} {u.Surname}") == normalizedSheetName
                || Normalize($"{u.Surname} {u.FirstName}") == normalizedSheetName);
        }

        private static bool TryReadDate(IXLCell cell, out DateTime date)
        {
            date = default;

            if (cell.DataType == XLDataType.DateTime)
            {
                date = cell.GetDateTime().Date;
                return true;
            }

            if (cell.DataType == XLDataType.Number)
            {
                var serialNumber = cell.GetDouble();

                try
                {
                    date = DateTime.FromOADate(serialNumber).Date;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            var text = cell.GetString().Trim();

            if (double.TryParse(
                    text.Replace(',', '.'),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var serialFromText))
            {
                try
                {
                    date = DateTime.FromOADate(serialFromText).Date;
                    return true;
                }
                catch
                {
                    // pokračuje se na běžné parsování textového data
                }
            }

            return DateTime.TryParse(
                text,
                CultureInfo.GetCultureInfo("cs-CZ"),
                DateTimeStyles.None,
                out date);
        }

        private static bool TryReadHours(IXLCell cell, out double hours)
        {
            hours = 0;

            if (cell.DataType == XLDataType.Number)
            {
                hours = cell.GetDouble();
                return true;
            }

            var text = cell.GetString().Trim().Replace(',', '.');

            return double.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out hours);
        }

        private static string Normalize(string value)
        {
            var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);

                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }

            return builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("  ", " ");
        }
    }

    public sealed class ExternalImportResult
    {
        public bool Success { get; set; }

        public int ImportedCount { get; set; }

        public int DeletedCount { get; set; }

        public List<string> Warnings { get; } = new();

        public List<string> Errors { get; } = new();
    }
}