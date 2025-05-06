using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;
using VykazyPrace.UserControls.CalendarV2;

namespace VykazyPrace.Dialogs
{
    public partial class TestDialog : Form
    {
        private DataTable? _loadedTable;


        public TestDialog()
        {
            InitializeComponent();
            DoubleBuffered = true;

            LoadFilteredData();

            customComboBox1.SetItems(new string[] { "test1", "test2", "paprika" });
        }

        private void LoadFilteredData()
        {
            string connectionString = "Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;";

            // Filtr podle Os. čísla a datumu
            string sqlQuery = @"
        SELECT * 
        FROM pwk.Prenos_pracovni_doby
        WHERE [Id_pracovníka (Os. číslo)] = @OsCislo";
            //AND [Datum směny] BETWEEN @StartDate AND @EndDate";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    // Parametry
                    command.Parameters.AddWithValue("@OsCislo", 1250);
                    //command.Parameters.AddWithValue("@StartDate", new DateTime(2025, 3, 24));
                    //command.Parameters.AddWithValue("@EndDate", new DateTime(2025, 3, 30));

                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridView1.DataSource = table;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dataGridView1.AutoResizeColumns();

                    _loadedTable = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání dat: " + ex.Message);
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
            LoadFilteredData();
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

    }
}
