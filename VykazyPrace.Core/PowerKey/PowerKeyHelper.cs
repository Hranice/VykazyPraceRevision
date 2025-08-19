using Microsoft.Data.SqlClient;
using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;

namespace VykazyPrace.Core.PowerKey
{
    public class PowerKeyHelper
    {
        private const string ConnectionString =
            "Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;";

        public async Task<int> DownloadForUserAsync(DateTime month, User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (user.PersonalNumber <= 0)
                throw new ArgumentException("Uživatel nemá vyplněné osobní číslo.", nameof(user));

            try
            {
                var data = await GetUserDataFromSpAsync(user.PersonalNumber, month);
                if (data == null || data.Rows.Count == 0) return 0;

                var repo = new ArrivalDepartureRepository();
                int saved = 0;

                foreach (DataRow row in data.Rows)
                {
                    if (!TryParseRowFlexible(row,
                                             out DateTime workDate,
                                             out DateTime? arrival,
                                             out DateTime? departure,
                                             out double worked,
                                             out double overtime,
                                             out string? reason))
                        continue;

                    // 1) Merge do existujícího záznamu stejného dne (pokud existuje)
                    var existing = await repo.GetByUserAndDateAsync(user.Id, workDate.Date);
                    if (existing != null)
                    {
                        bool changed = false;

                        if (!existing.ArrivalTimestamp.HasValue && arrival.HasValue)
                        { existing.ArrivalTimestamp = arrival; changed = true; }

                        if (!existing.DepartureTimestamp.HasValue && departure.HasValue)
                        { existing.DepartureTimestamp = departure; changed = true; }

                        if (existing.HoursWorked == 0 && worked > 0)
                        {
                            existing.HoursWorked = worked;
                            changed = true;
                        }

                        if (existing.HoursOvertime == 0 && overtime > 0)
                        {
                            existing.HoursOvertime = overtime;
                            changed = true;
                        }

                        if (string.IsNullOrWhiteSpace(existing.DepartureReason) && !string.IsNullOrWhiteSpace(reason))
                        { existing.DepartureReason = reason; changed = true; }

                        if (changed)
                            await repo.UpdateArrivalDepartureAsync(existing);

                        continue; // hotovo pro tento řádek
                    }

                    // 2) Neexistuje záznam – kontrola duplicit jen pokud máme komplet pár
                    if (arrival.HasValue && departure.HasValue)
                    {
                        var dup = await repo.GetExactMatchAsync(
                            user.Id, workDate, arrival.Value, departure.Value, worked, overtime);
                        if (dup != null) continue;
                    }

                    // 3) Vlož nový záznam (povoleno i s jednostranným časem)
                    var entity = new ArrivalDeparture
                    {
                        UserId = user.Id,
                        WorkDate = workDate.Date,
                        ArrivalTimestamp = arrival,
                        DepartureTimestamp = departure,
                        DepartureReason = reason,
                        HoursWorked = worked,
                        HoursOvertime = overtime
                    };

                    await repo.CreateArrivalDepartureAsync(entity);
                    saved++;
                }

                return saved;
            }
            catch (Exception ex)
            {
                throw new Exception($"Chyba při zpracování uživatele {user?.PersonalNumber}.", ex);
            }
        }


        // === PŮVODNÍ API ponecháno kvůli kompatibilitě (jede přes všechny uživatele) ===
        // Pokud ho už nepotřebuješ, můžeš si ho klidně odstranit až ve chvíli,
        // kdy refaktor dokončíš i na volající straně.
        public async Task<int> DownloadArrivalsDeparturesAsync(DateTime month)
        {
            try
            {
                // zachováno původní chování: jede přes všechny uživatele přes SaveToDatabaseAsync
                return await SaveToDatabaseAsync(month);
            }
            catch (Exception ex)
            {
                throw new Exception("Nepodařilo se stáhnout příchody a odchody.", ex);
            }
        }

        // === PONECHÁNO: metoda pro souhrn odpracovaných hodin (dočasné view) ===
        public async Task<Dictionary<int, double>> GetWorkedHoursByPersonalNumberForMonthAsync(DateTime month)
        {
            string viewName = $"vw_Prenos_tmp_{Guid.NewGuid():N}";
            await CreateTemporaryViewAsync(month, viewName);

            try
            {
                return await GetWorkedHoursFromViewAsync(viewName);
            }
            finally
            {
                await DropTemporaryViewAsync(viewName);
            }
        }

        // ====== INTERNÍ POMOCNÉ METODY ======

        // Nové čtení dat pro jednoho uživatele přes SP [pwk].[Prenos_pracovni_doby_raw]
        private async Task<DataTable?> GetUserDataFromSpAsync(int personalNumber, DateTime monthDate)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand("[pwk].[Prenos_pracovni_doby_raw]", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };

                command.Parameters.Add("@MonthDate", SqlDbType.Date).Value = monthDate.Date;
                command.Parameters.Add("@PersonalNum", SqlDbType.NVarChar, 50).Value = personalNumber.ToString();

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var table = new DataTable();
                table.Load(reader);
                return table;
            }
            catch (Exception ex)
            {
                throw new Exception($"Chyba při čtení dat pro osobní číslo {personalNumber}.", ex);
            }
        }

        // Zachováno: helper pro dočasné view na součty hodin (používá AttnMonth/AttnDay)
        private async Task CreateTemporaryViewAsync(DateTime month, string viewName)
        {
            string dateToMonth = $"[pwk].[DateToMonthNumber] ('{month:yyyy-MM-dd}')";
            string viewSql = $@"
CREATE VIEW [pwk].[{viewName}]
AS
SELECT
    [PE].[PersonalNum] AS [Id_pracovníka (Os. číslo)],
    [AD].[WorkedHours]/60. AS [Počet hodin (standard)]
FROM [pwk].[Person] [PE]
INNER JOIN [pwk].[AttnMonth] [AM] ON [AM].[PersonID] = [PE].[PersonID]
INNER JOIN [pwk].[AttnDay] [AD] ON [AD].[AttnMonthID] = [AM].[AttnMonthID]
WHERE [PE].[DeletedID] = 0 
AND [AM].[MonthNumber] = {dateToMonth}";

            const string setFormat = "SET DATEFORMAT DMY;";

            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            using (var cmd1 = new SqlCommand(setFormat, connection))
                await cmd1.ExecuteNonQueryAsync();

            using (var cmd2 = new SqlCommand(viewSql, connection))
                await cmd2.ExecuteNonQueryAsync();
        }

        private async Task DropTemporaryViewAsync(string viewName)
        {
            string sql = $"DROP VIEW IF EXISTS [pwk].[{viewName}]";
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<Dictionary<int, double>> GetWorkedHoursFromViewAsync(string viewName)
        {
            string sql = $@"
SELECT [Id_pracovníka (Os. číslo)] AS PersonalNumber,
       SUM([Počet hodin (standard)]) AS TotalHours
FROM [pwk].[{viewName}]
GROUP BY [Id_pracovníka (Os. číslo)]";

            var result = new Dictionary<int, double>();

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                object personalObj = reader.GetValue(0);
                int pn;

                if (personalObj is int directPn)
                    pn = directPn;
                else if (personalObj is string strPn && int.TryParse(strPn, out int parsedPn))
                    pn = parsedPn;
                else
                    continue;

                double hours = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader.GetValue(1));
                result[pn] = hours;
            }

            return result;
        }

        private async Task<int> SaveToDatabaseAsync(DateTime month)
        {
            try
            {
                var userRepo = new UserRepository();
                var arrivalRepo = new ArrivalDepartureRepository();
                var users = await userRepo.GetAllUsersAsync();

                int totalSaved = 0;

                foreach (var user in users)
                {
                    if (user.PersonalNumber <= 0) continue;

                    var table = await GetUserDataFromSpAsync(user.PersonalNumber, month);
                    if (table == null || table.Rows.Count == 0) continue;

                    foreach (DataRow row in table.Rows)
                    {
                        try
                        {
                            if (!TryParseRowFlexible(row,
                                                     out var workDate,
                                                     out var arrival,
                                                     out var departure,
                                                     out var worked,
                                                     out var overtime,
                                                     out var reason))
                                continue;

                            // 1) Merge do existujícího záznamu dne
                            var existing = await arrivalRepo.GetByUserAndDateAsync(user.Id, workDate.Date);
                            if (existing != null)
                            {
                                bool changed = false;

                                if (!existing.ArrivalTimestamp.HasValue && arrival.HasValue)
                                { existing.ArrivalTimestamp = arrival; changed = true; }

                                if (!existing.DepartureTimestamp.HasValue && departure.HasValue)
                                { existing.DepartureTimestamp = departure; changed = true; }

                                if (existing.HoursWorked == 0 && worked > 0)
                                {
                                    existing.HoursWorked = worked;
                                    changed = true;
                                }

                                if (existing.HoursOvertime == 0 && overtime > 0)
                                {
                                    existing.HoursOvertime = overtime;
                                    changed = true;
                                }

                                if (string.IsNullOrWhiteSpace(existing.DepartureReason) && !string.IsNullOrWhiteSpace(reason))
                                { existing.DepartureReason = reason; changed = true; }

                                if (changed)
                                    await arrivalRepo.UpdateArrivalDepartureAsync(existing);

                                continue;
                            }

                            // 2) Neexistuje záznam – kontrola duplicit, když máme komplet pár
                            if (arrival.HasValue && departure.HasValue)
                            {
                                var dup = await arrivalRepo.GetExactMatchAsync(
                                    user.Id, workDate, arrival.Value, departure.Value, worked, overtime);
                                if (dup != null) continue;
                            }

                            // 3) Vlož nový
                            var newEntry = new ArrivalDeparture
                            {
                                UserId = user.Id,
                                WorkDate = workDate.Date,
                                ArrivalTimestamp = arrival,
                                DepartureTimestamp = departure,
                                DepartureReason = reason,
                                HoursWorked = worked,
                                HoursOvertime = overtime
                            };

                            await arrivalRepo.CreateArrivalDepartureAsync(newEntry);
                            totalSaved++;
                        }
                        catch (Exception exRow)
                        {
                            throw new Exception($"Chyba při ukládání záznamu pro uživatele {user.PersonalNumber}", exRow);
                        }
                    }
                }

                return totalSaved;
            }
            catch (Exception ex)
            {
                throw new Exception("Chyba při globálním zpracování dat.", ex);
            }
        }


        // === Parsování řádků (původní i flexibilní varianta) ===

        // Původní signatura, kdyby ji něco volalo – ponecháno kvůli zpětné kompatibilitě:
        private bool TryParseArrivalDepartureRow(DataRow row, out DateTime arrival, out DateTime departure, out DateTime workDate, out double worked, out double overtime, out string? reason)
        {
            arrival = departure = workDate = default;
            worked = overtime = 0;
            reason = row["Důvod odchodu"]?.ToString();

            string? arrivalStr = row.Table.Columns.Contains("Příchod") ? row["Příchod"]?.ToString() : null;
            string? departureStr = row.Table.Columns.Contains("Odchod") ? row["Odchod"]?.ToString() : null;
            string? dateStr = row.Table.Columns.Contains("Datum směny") ? row["Datum směny"]?.ToString() : null;
            string? workedStr = row.Table.Columns.Contains("Počet hodin (standard)") ? row["Počet hodin (standard)"]?.ToString()?.Replace(',', '.') : null;
            string? overtimeStr = row.Table.Columns.Contains("Počet hodin (přesčas)") ? row["Počet hodin (přesčas)"]?.ToString()?.Replace(',', '.') : null;

            return DateTime.TryParse(arrivalStr, out arrival)
                && DateTime.TryParse(departureStr, out departure)
                && DateTime.TryParse(dateStr, out workDate)
                && double.TryParse(workedStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out worked)
                && double.TryParse(overtimeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out overtime);
        }

        // Flexibilní varianta (pro nový „nepárový“ zdroj)
        private static bool TryParseRowFlexible(
            DataRow row,
            out DateTime workDate,
            out DateTime? arrival,
            out DateTime? departure,
            out double worked,
            out double overtime,
            out string? reason)
        {
            workDate = default;
            arrival = null;
            departure = null;
            worked = 0;
            overtime = 0;
            reason = row.Table.Columns.Contains("Důvod odchodu")
                     ? row["Důvod odchodu"]?.ToString()
                     : null;

            if (!TryGetDateTime(row, "Datum směny", out workDate))
                return false;

            if (TryGetDateTime(row, "Příchod", out var tmpArr))
                arrival = tmpArr;

            if (TryGetDateTime(row, "Odchod", out var tmpDep))
                departure = tmpDep;

            worked = TryGetDouble(row, "Počet hodin (standard)", out var w) ? w : 0;
            overtime = TryGetDouble(row, "Počet hodin (přesčas)", out var ot) ? ot : 0;

            return true;
        }

        private static bool TryGetDateTime(DataRow row, string col, out DateTime value)
        {
            value = default;
            if (!row.Table.Columns.Contains(col)) return false;
            var obj = row[col];
            if (obj == null || obj == DBNull.Value) return false;
            return DateTime.TryParse(obj.ToString(), out value);
        }

        private static bool TryGetDouble(DataRow row, string col, out double value)
        {
            value = 0;
            if (!row.Table.Columns.Contains(col)) return false;
            var s = row[col]?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Replace(',', '.');
            return double.TryParse(s, System.Globalization.NumberStyles.Any,
                                   System.Globalization.CultureInfo.InvariantCulture, out value);
        }
    }
}
