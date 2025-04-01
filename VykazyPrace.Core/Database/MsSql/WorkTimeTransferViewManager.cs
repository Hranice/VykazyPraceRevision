using System.Data;

namespace VykazyPrace.Core.Database.MsSql
{
    public class WorkTimeTransferViewManager
    {
        private readonly MsSqlService _sqlService;

        public WorkTimeTransferViewManager(MsSqlService sqlService)
        {
            _sqlService = sqlService;
        }

        public void DropView()
        {
            _sqlService.ExecuteNonQuery("DROP VIEW IF EXISTS pwk.Prenos_pracovni_doby");
        }

        public void CreateView()
        {
            string setDateFormat = "SET DATEFORMAT DMY;";
            string createView = @"
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
    SELECT TOP (1) *
    FROM [pwk].[AttnDay_Registration] [AR]
    WHERE [AR].[AttnDayID] = [AD].[AttnDayID]
    ORDER BY [AR].[RegistrationTime] DESC
) [D2]
INNER JOIN [pwk].[Denote] [D] ON [D2].[DenoteID] = [D].[DenoteID]
CROSS APPLY (
    SELECT TOP (1) [RegistrationTime], [DenoteName]
    FROM [pwk].[AttnDay_Registration] [AR2]
    WHERE [AR2].[AttnDayID] = [D2].[AttnDayID] AND [D].[InOutType] = 2 AND [AR2].[DeletedID] = 0
    ORDER BY [AR2].[RegistrationTime] ASC
) AS [PB]
WHERE [PE].[DeletedID] = 0
AND [AM].[MonthNumber] = ([pwk].[DateToMonthNumber](GETDATE()))
";

            _sqlService.ExecuteNonQueryMultiple(setDateFormat, createView);
        }

        public List<WorkTimeTransferRecord> LoadByPersonalNumber(int personalNumber)
        {
            string sql = @"
SELECT * 
FROM pwk.Prenos_pracovni_doby
WHERE [Id_pracovníka (Os. číslo)] = @OsCislo";

            var table = _sqlService.ExecuteQuery(sql, new Dictionary<string, object>
    {
        { "@OsCislo", personalNumber }
    });

            if (table == null || table.Rows.Count == 0)
                return new List<WorkTimeTransferRecord>(); // 👈 bezpečný návrat

            return table.AsEnumerable().Select(row => new WorkTimeTransferRecord
            {
                PersonId = Convert.ToInt32(row["Klíč_pracovníka (PersonID)"]),
                PersonalNumber = Convert.ToInt32(row["Id_pracovníka (Os. číslo)"]),
                Arrival = row.Field<DateTime?>("Příchod"),
                Departure = row.Field<DateTime?>("Odchod"),
                DepartureReason = row["Důvod odchodu"]?.ToString(),
                StandardHours = Convert.ToDouble(row["Počet hodin (standard)"]),
                OvertimeHours = Convert.ToDouble(row["Počet hodin (přesčas)"]),
                WorkDate = Convert.ToDateTime(row["Datum směny"]),
                ApprovalState = row["Stav schválení měsíce"]?.ToString()
            }).ToList();

        }

    }
}
