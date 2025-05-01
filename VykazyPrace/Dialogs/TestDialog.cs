using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Logging;
using VykazyPrace.UserControls.CalendarV2;

namespace VykazyPrace.Dialogs
{
    public partial class TestDialog : Form
    {
        private CalendarV2 _calendar;

        public TestDialog()
        {
            InitializeComponent();
            DoubleBuffered = true;
            KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání dat: " + ex.Message);
            }
        }


        private async void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                await _calendar.DeleteRecord();
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
AND [AM].[MonthNumber] = ([pwk].[DateToMonthNumber] (GETDATE()))";

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
    }
}
