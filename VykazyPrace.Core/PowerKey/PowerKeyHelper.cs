using Microsoft.Data.SqlClient;
using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;

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
            {
                AppLogger.Debug("Uživatel nemá vyplněné osobní číslo.");
                return 0;
            }

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

                    var existingForDay = await repo.ListByUserAndDateAsync(user.Id, workDate.Date);

                    // Helpery
                    static bool SameDT(DateTime? a, DateTime? b, TimeSpan? tol = null)
                    {
                        if (!a.HasValue && !b.HasValue) return true;
                        if (a.HasValue != b.HasValue) return false;
                        var tolerance = tol ?? TimeSpan.FromSeconds(1);
                        return Math.Abs((a.Value - b.Value).TotalSeconds) <= tolerance.TotalSeconds;
                    }

                    static bool SameDouble(double a, double b, double eps = 0.01)
                        => Math.Abs(a - b) < eps;

                    static bool IsSubset(ArrivalDeparture ex,
                                         DateTime? arr,
                                         DateTime? dep,
                                         double worked,
                                         double overtime,
                                         string? reason)
                    {
                        bool arrivalOk = ex.ArrivalTimestamp == null || (arr.HasValue && SameDT(ex.ArrivalTimestamp, arr));
                        bool departureOk = ex.DepartureTimestamp == null || (dep.HasValue && SameDT(ex.DepartureTimestamp, dep));
                        bool workedOk = ex.HoursWorked == 0 || SameDouble(ex.HoursWorked, worked);
                        bool overtimeOk = ex.HoursOvertime == 0 || SameDouble(ex.HoursOvertime, overtime);
                        bool reasonOk = string.IsNullOrWhiteSpace(ex.DepartureReason) ||
                                        string.Equals(ex.DepartureReason, reason ?? string.Empty, StringComparison.OrdinalIgnoreCase);

                        return arrivalOk && departureOk && workedOk && overtimeOk && reasonOk
                               && (
                                      (ex.DepartureTimestamp == null && dep.HasValue) ||
                                      (!SameDouble(ex.HoursWorked, worked) && worked > 0) ||
                                      (!SameDouble(ex.HoursOvertime, overtime) && overtime > 0) ||
                                      (string.IsNullOrWhiteSpace(ex.DepartureReason) && !string.IsNullOrWhiteSpace(reason))
                                  );
                    }

                    var exact = existingForDay.FirstOrDefault(x =>
                        SameDT(x.ArrivalTimestamp, arrival) &&
                        SameDT(x.DepartureTimestamp, departure) &&
                        SameDouble(x.HoursWorked, worked) &&
                        SameDouble(x.HoursOvertime, overtime) &&
                        string.Equals(x.DepartureReason ?? string.Empty,
                                      reason ?? string.Empty,
                                      StringComparison.OrdinalIgnoreCase));

                    if (exact != null)
                    {
                        // už tam je identický záznam
                        continue;
                    }

                    var subset = existingForDay.FirstOrDefault(x => IsSubset(x, arrival, departure, worked, overtime, reason));
                    if (subset != null)
                    {
                        subset.ArrivalTimestamp = subset.ArrivalTimestamp ?? arrival;
                        subset.DepartureTimestamp = subset.DepartureTimestamp ?? departure;
                        if (subset.HoursWorked == 0 && worked > 0) subset.HoursWorked = worked;
                        if (subset.HoursOvertime == 0 && overtime > 0) subset.HoursOvertime = overtime;
                        if (string.IsNullOrWhiteSpace(subset.DepartureReason) && !string.IsNullOrWhiteSpace(reason))
                            subset.DepartureReason = reason;

                        await repo.UpdateArrivalDepartureAsync(subset);
                        saved++;
                        continue;
                    }

                    var entity = new ArrivalDeparture
                    {
                        UserId = user.Id,
                        WorkDate = workDate.Date,
                        ArrivalTimestamp = arrival,
                        DepartureTimestamp = departure,
                        DepartureReason = string.IsNullOrWhiteSpace(reason) ? null : reason,
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
                AppLogger.Error($"Chyba při zpracování uživatele {user?.PersonalNumber}.", ex);
                return 0;
            }
        }


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
                AppLogger.Error($"Chyba při čtení dat pro osobní číslo {personalNumber}.", ex);
                return null;
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
