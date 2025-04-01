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
            _sqlService.ExecuteNonQuery("DROP VIEW IF EXISTS pwk.WorkTimeTransfer");
        }

        public void CreateView()
        {
            string setDateFormat = "SET DATEFORMAT DMY;";
            string createView = @"
CREATE VIEW [pwk].[WorkTimeTransfer]
AS
SELECT
    [PE].[PersonID] AS [PersonId],
    [PE].[PersonalNum] AS [PersonalNumber],
    CASE [PB].[RegistrationTime] WHEN [D2].[RegistrationTime] THEN NULL ELSE [PB].[RegistrationTime] END AS [Arrival],
    [D2].[RegistrationTime] AS [Departure],
    [pwk].[GetLocalName] ([PB].[DenoteName],5) as [DepartureReason],
    [AD].[WorkedHours]/60. AS [StandardHours],
    [AD].[BalanceHours]/60. AS [OvertimeHours],
    [pwk].[DayNumberToDate] ([AD].[DayNumber]) AS [WorkDate],
    CASE [AM].[ApproveState]
        WHEN 0 THEN 'Unprocessed'
        WHEN 1 THEN 'Processed'
        WHEN 4 THEN 'Approved by Supervisor'
        WHEN 7 THEN 'Approved by HR'
    END AS [ApprovalState]
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
AND [AM].[MonthNumber] = ([pwk].[DateToMonthNumber](GETDATE()))";

            _sqlService.ExecuteNonQueryMultiple(setDateFormat, createView);
        }

        public List<WorkTimeTransferRecord> LoadByPersonalNumber(int personalNumber)
        {
            string sql = @"
SELECT * 
FROM pwk.WorkTimeTransfer
WHERE [PersonalNumber] = @PersonalNumber";

            var table = _sqlService.ExecuteQuery(sql, new Dictionary<string, object>
            {
                { "@PersonalNumber", personalNumber }
            });

            return table.AsEnumerable().Select(row => new WorkTimeTransferRecord
            {
                PersonId = row.Field<int>("PersonId"),
                PersonalNumber = row.Field<int>("PersonalNumber"),
                Arrival = row.Field<DateTime?>("Arrival"),
                Departure = row.Field<DateTime?>("Departure"),
                DepartureReason = row.Field<string>("DepartureReason"),
                StandardHours = row.Field<double>("StandardHours"),
                OvertimeHours = row.Field<double>("OvertimeHours"),
                WorkDate = row.Field<DateTime>("WorkDate"),
                ApprovalState = row.Field<string>("ApprovalState")
            }).ToList();
        }
    }
}
