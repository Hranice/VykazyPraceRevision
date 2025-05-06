using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;

namespace VykazyPrace.Core.PowerKey
{
    public class PowerKeyHelper
    {
        private const string ConnectionString = "Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;";


        // TODO: Vzít pouze pokud je stav Nezpracováno (prnvě by bylo fajn zjistit, co to znamená)
        public async Task<int> DownloadArrivalsDeparturesAsync(DateTime month)
        {
            try
            {
                DropView();
                CreateView(month);

                return await SaveToDatabaseAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Nepodařilo se stáhnout příchody a odchody.", ex);
            }
        }

        private void DropView()
        {
            string dropQuery = "DROP VIEW IF EXISTS pwk.Prenos_pracovni_doby";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(dropQuery, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Chyba při odstraňování view.", ex);
            }
        }

        private void CreateView(DateTime targetMonth)
        {
            const string dateFormatCommand = "SET DATEFORMAT DMY;";

            // SQL část pro výpočet monthNumber pomocí PowerKey funkce
            string dateToMonthNumber = $"[pwk].[DateToMonthNumber] ('{targetMonth:yyyy-MM-dd}')";

            string createViewCommand = $@"
CREATE VIEW [pwk].[Prenos_pracovni_doby]
AS
SELECT
    [PE].[PersonID] AS [Klíč_pracovníka (PersonID)],
    [PE].[PersonalNum] AS [Id_pracovníka (Os. číslo)],
    CASE [PB].[RegistrationTime] WHEN [D2].[RegistrationTime] THEN NULL ELSE [PB].[RegistrationTime] END AS [Příchod],
    [D2].[RegistrationTime] AS [Odchod],
    [pwk].[GetLocalName] ([PB].[DenoteName],5) as [Důvod odchodu],
    [AD].[WorkedHours]/60. AS [Počet hodin (standard)],
    [AD].[BalanceHours]/60. AS [Počet hodin (přesčas)],
    [pwk].[DayNumberToDate] ([AD].[DayNumber]) AS [Datum směny],
    CASE [AM].[ApproveState]
        WHEN 0 THEN 'Nezpracováno'
        WHEN 1 THEN 'Zpracováno'
        WHEN 4 THEN 'Schváleno vedoucím'
        WHEN 7 THEN 'Schváleno HR'
    END AS [Stav schválení měsíce]
FROM [pwk].[Person] [PE]
INNER JOIN [pwk].[AttnMonth] [AM] ON [AM].[PersonID] = [PE].[PersonID]
INNER JOIN [pwk].[AttnDay] [AD] ON [AD].[AttnMonthID] = [AM].[AttnMonthID]
CROSS APPLY (
    SELECT TOP (1) * FROM [pwk].[AttnDay_Registration] [AR]
    WHERE [AR].[AttnDayID]=[AD].[AttnDayID]
    ORDER BY [AR].[RegistrationTime] DESC
) [D2]
INNER JOIN [pwk].[Denote] [D] ON [D2].[DenoteID] = [D].[DenoteID]
CROSS APPLY (
    SELECT TOP (1) [RegistrationTime], [DenoteName]
    FROM [pwk].[AttnDay_Registration] [AR2]
    WHERE [AR2].[AttnDayID]=[D2].[AttnDayID] AND [D].[InOutType]=2 AND [AR2].[DeletedID]=0
    ORDER BY [AR2].[RegistrationTime] ASC
) AS [PB]
WHERE [PE].[DeletedID] = 0 
AND [AM].[MonthNumber] = {dateToMonthNumber}";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                using (var setFormat = new SqlCommand(dateFormatCommand, connection))
                {
                    setFormat.ExecuteNonQuery();
                }

                using (var createView = new SqlCommand(createViewCommand, connection))
                {
                    createView.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Chyba při vytváření view.", ex);
            }
        }



        private async Task<int> SaveToDatabaseAsync()
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

                    var userData = await GetUserArrivalDepartureDataAsync(user.PersonalNumber);
                    if (userData == null) continue;

                    foreach (DataRow row in userData.Rows)
                    {
                        try
                        {
                            if (!TryParseArrivalDepartureRow(row, out var arrival, out var departure, out var workDate, out var worked, out var overtime, out var reason))
                                continue;

                            var duplicate = await arrivalRepo.GetExactMatchAsync(user.Id, workDate, arrival, departure, worked, overtime);
                            if (duplicate != null)
                                continue;

                            var latestDate = await arrivalRepo.GetLatestWorkDateAsync(user.Id);
                            if (latestDate.HasValue && workDate <= latestDate.Value)
                                continue;


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

        private async Task<DataTable?> GetUserArrivalDepartureDataAsync(int personalNumber)
        {
            const string sqlQuery = @"
                SELECT * 
                FROM pwk.Prenos_pracovni_doby
                WHERE [Id_pracovníka (Os. číslo)] = @OsCislo";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@OsCislo", personalNumber);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                var table = new DataTable();
                table.Load(reader);
                return table;
            }
            catch (Exception ex)
            {
                throw new Exception($"Chyba při zpracování uživatele {personalNumber}", ex);
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

            // 1. Nastavení formátu data (samostatně)
            using (var cmd1 = new SqlCommand(setFormat, connection))
            {
                await cmd1.ExecuteNonQueryAsync();
            }

            // 2. Vytvoření view (musí být samostatně)
            using (var cmd2 = new SqlCommand(viewSql, connection))
            {
                await cmd2.ExecuteNonQueryAsync();
            }
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



        private bool TryParseArrivalDepartureRow(DataRow row, out DateTime arrival, out DateTime departure, out DateTime workDate, out double worked, out double overtime, out string? reason)
        {
            arrival = departure = workDate = default;
            worked = overtime = 0;
            reason = row["Důvod odchodu"]?.ToString();

            string? arrivalStr = row["Příchod"]?.ToString();
            string? departureStr = row["Odchod"]?.ToString();
            string? dateStr = row["Datum směny"]?.ToString();
            string? workedStr = row["Počet hodin (standard)"]?.ToString()?.Replace(',', '.');
            string? overtimeStr = row["Počet hodin (přesčas)"]?.ToString()?.Replace(',', '.');

            return DateTime.TryParse(arrivalStr, out arrival)
                && DateTime.TryParse(departureStr, out departure)
                && DateTime.TryParse(dateStr, out workDate)
                && double.TryParse(workedStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out worked)
                && double.TryParse(overtimeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out overtime);
        }
    }
}
