using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;
using VykazyPrace.UserControls.CalendarV2;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace VykazyPrace.Dialogs
{
    public partial class TestDialog : Form
    {
        private DataTable? _loadedTable;


        public TestDialog()
        {
            InitializeComponent();
            DoubleBuffered = true;

            LoadFilteredData(DateTime.Now, 1250);

            customComboBox1.SetItems(new string[] { "test1", "test2", "paprika" });
        }

        private void LoadFilteredData(DateTime monthDate, int personalNumber)
        {
            string cs = "Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;";

            try
            {
                using var connection = new SqlConnection(cs);
                using var command = new SqlCommand("[pwk].[Prenos_pracovni_doby_raw]", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@MonthDate", SqlDbType.Date).Value = monthDate.Date;
                command.Parameters.Add("@PersonalNum", SqlDbType.NVarChar, 15).Value = personalNumber.ToString();

                using var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                dataGridView1.DataSource = table;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dataGridView1.AutoResizeColumns();

                _loadedTable = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání dat: " + ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void DropView()
        {
            string connectionString = "Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;";
            string dropQuery = "DROP VIEW IF EXISTS pwk.Prenos_pracovni_doby";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(dropQuery, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("View bylo odstraněno.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při odstraňování view: " + ex.Message);
            }
        }

        private void CreateView()
        {
            string connectionString = "Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;";

            string dateFormatCommand = "SET DATEFORMAT DMY;"; // spustíme zvlášť

            string createViewCommand = @"
CREATE VIEW [pwk].[Prenos_pracovni_doby]
AS


SELECT
[PE].[PersonID] AS [Klíč_pracovníka (PersonID)]
,[PE].[PersonalNum] AS [Id_pracovníka (Os. číslo)]
,CASE [PB].[RegistrationTime] WHEN [D2].[RegistrationTime] THEN NULL ELSE [PB].[RegistrationTime] END AS [Příchod]
,[D2].[RegistrationTime] AS [Odchod]
,[pwk].[GetLocalName] ([PB].[DenoteName],5) as [Důvod odchodu]
,[AD].[WorkedHours]/60. AS [Počet hodin (standard)]
,[AD].[BalanceHours]/60. AS [Počet hodin (přesčas)]
,[pwk].[DayNumberToDate] ([AD].[DayNumber]) AS [Datum směny]
,CASE [AM].[ApproveState] WHEN 0 THEN 'Nezpracováno' WHEN 1 THEN 'Zpracováno' WHEN 4 THEN 'Schváleno vedoucím' WHEN 7 THEN 'Schváleno HR' END AS [Stav schválení měsíce]
FROM 
[pwk].[Person] [PE]
INNER JOIN [pwk].[AttnMonth] [AM] ON [AM].[PersonID] = [PE].[PersonID]
INNER JOIN [pwk].[AttnDay] [AD] ON [AD].[AttnMonthID] = [AM].[AttnMonthID]
CROSS APPLY
(   SELECT TOP (1) *
FROM [pwk].[AttnDay_Registration] [AR]
WHERE [AR].[AttnDayID]=[AD].[AttnDayID]
ORDER BY [AR].[RegistrationTime] DESC) [D2]

INNER JOIN [pwk].[Denote] [D] ON [d2].[DenoteID] = [D].[DenoteID]


CROSS APPLY
(   SELECT TOP (1) [RegistrationTime],[DenoteName] 
FROM [pwk].[AttnDay_Registration] [AR2]
WHERE [AR2].[AttnDayID]=[D2].[AttnDayID] AND [D].[InOutType]=2 AND [AR2].[DeletedID]=0
ORDER BY [AR2].[RegistrationTime] ASC) AS [PB]

WHERE [PE].[DeletedID] = 0 
AND [AM].[MonthNumber] = ([pwk].[DateToMonthNumber] (GETDATE())) - 1";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Nejprve SET DATEFORMAT
                    using (SqlCommand setFormat = new SqlCommand(dateFormatCommand, connection))
                    {
                        setFormat.ExecuteNonQuery();
                    }

                    // Potom CREATE VIEW
                    using (SqlCommand createView = new SqlCommand(createViewCommand, connection))
                    {
                        createView.ExecuteNonQuery();
                    }

                    MessageBox.Show("View bylo úspěšně vytvořeno.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vytváření view: " + ex.Message);
            }
        }


        private void buttonDrop_Click(object sender, EventArgs e)
        {
            DropView();
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            CreateView();
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            LoadFilteredData(DateTime.Now, 1250);
        }

        private async void buttonSave_Click(object sender, EventArgs e)
        {
            if (_loadedTable == null || _loadedTable.Rows.Count == 0)
            {
                MessageBox.Show("Žádná data k uložení.");
                return;
            }

            try
            {
                var repo = new ArrivalDepartureRepository();
                var userRepo = new UserRepository();
                var result = await userRepo.GetUserByWindowsUsernameAsync("jprochazka");
                int targetUserId = result.Id;
                int saved = 0;

                foreach (DataRow row in _loadedTable.Rows)
                {
                    try
                    {
                        // Získání hodnot
                        var arrivalStr = row["Příchod"]?.ToString();
                        var departureStr = row["Odchod"]?.ToString();
                        var reason = row["Důvod odchodu"]?.ToString();
                        var workedStr = row["Počet hodin (standard)"]?.ToString()?.Replace(',', '.');
                        var overtimeStr = row["Počet hodin (přesčas)"]?.ToString()?.Replace(',', '.');
                        var dateStr = row["Datum směny"]?.ToString();

                        // Parsování
                        if (!DateTime.TryParse(arrivalStr, out DateTime arrival)) continue;
                        if (!DateTime.TryParse(departureStr, out DateTime departure)) continue;
                        if (!DateTime.TryParse(dateStr, out DateTime workDate)) continue;
                        if (!double.TryParse(workedStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double worked)) continue;
                        if (!double.TryParse(overtimeStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double overtime)) continue;

                        // Kontrola existence záznamu pro daného uživatele a den
                        var existing = await repo.GetByUserAndDateAsync(targetUserId, workDate);
                        if (existing != null) continue;

                        var newEntry = new ArrivalDeparture
                        {
                            UserId = targetUserId,
                            WorkDate = workDate.Date,
                            ArrivalTimestamp = arrival,
                            DepartureTimestamp = departure,
                            DepartureReason = reason,
                            HoursWorked = worked,
                            HoursOvertime = overtime
                        };

                        await repo.CreateArrivalDepartureAsync(newEntry);
                        saved++;
                    }
                    catch (Exception exRow)
                    {
                        MessageBox.Show("Chyba při zpracování řádku: " + exRow.Message);
                    }
                }

                MessageBox.Show($"Uloženo záznamů: {saved}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při ukládání dat: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // ===== ČTENÍ KALENDÁŘE PŘES OUTLOOK OOM =====
            // Cíl: Načíst nadcházející události (např. na 7 dní dopředu) z výchozího kalendáře.

            Microsoft.Office.Interop.Outlook.Application outlook = null;
            Microsoft.Office.Interop.Outlook.MAPIFolder calendarFolder = null;
            Microsoft.Office.Interop.Outlook.Items items = null;

            try
            {
                // 1) Spustíme/napojíme se na instanci Outlooku
                outlook = new Microsoft.Office.Interop.Outlook.Application();

                // 2) Získáme session a výchozí kalendář
                var session = outlook.Session; // Current Session (MAPI)
                calendarFolder = session.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderCalendar);

                // 3) Získáme kolekci položek a připravíme ji pro filtrování
                items = calendarFolder.Items;

                // Včetně opakovaných (recurrence) a seřadíme podle startu
                items.IncludeRecurrences = true;
                items.Sort("[Start]");

                // 4) Omezíme rozsah – třeba od teď do +7 dní (Outlook očekává US formát datumu)
                var start = DateTime.Now;
                var end = DateTime.Now.AddDays(7);

                // Pozor na formát dat – Outlook Restrict používá M/d/yyyy h:mm tt (EN-US)
                string filter = $"[Start] >= '{start.ToString("g", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}' AND " +
                                $"[Start] <= '{end.ToString("g", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}'";

                var restricted = items.Restrict(filter);

                // 5) Promítneme do UI (např. ListBox)
                listBoxEvents.Items.Clear();

                foreach (object obj in restricted)
                {
                    if (obj is Microsoft.Office.Interop.Outlook.AppointmentItem appt)
                    {
                        // Bezpečně přečteme základní info
                        string subject = string.IsNullOrWhiteSpace(appt.Subject) ? "(bez názvu)" : appt.Subject;
                        string when = $"{appt.Start:g} – {appt.End:t}";
                        string where = string.IsNullOrWhiteSpace(appt.Location) ? "" : $" @ {appt.Location}";

                        listBoxEvents.Items.Add($"{when} | {subject}{where}");
                    }
                }

                if (listBoxEvents.Items.Count == 0)
                    listBoxEvents.Items.Add("V daném období nebyly nalezeny žádné události.");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, "Chyba při čtení kalendáře z Outlooku:\n" + ex.Message,
                    "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Uvolníme COM objekty – snížíme riziko „visících“ procesů
                if (items != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(items);
                if (calendarFolder != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(calendarFolder);
                if (outlook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(outlook);
            }
        }
    }
}
