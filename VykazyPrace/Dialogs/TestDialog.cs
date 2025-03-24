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
        }

        private void LoadFilteredData()
        {
            string connectionString = "Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;";

            // Filtr podle Os. čísla a datumu
            string sqlQuery = @"
        SELECT * 
        FROM pwk.Prenos_pracovni_doby
        WHERE [Id_pracovníka (Os. číslo)] = @OsCislo
        AND [Datum směny] BETWEEN @StartDate AND @EndDate";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    // Parametry
                    command.Parameters.AddWithValue("@OsCislo", 1250);
                    command.Parameters.AddWithValue("@StartDate", new DateTime(2025, 2, 24));
                    command.Parameters.AddWithValue("@EndDate", new DateTime(2025, 2, 28));

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

        private void TestDialog_Load(object sender, EventArgs e)
        {
            //_calendar = new UserControls.CalendarV2.CalendarV2(_currentUser);
            //_calendar.Dock = DockStyle.Fill;
            //_calendar.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            //_calendar.Location = new Point(0, 0);
            //_calendar.Name = "calendarV21";
            //_calendar.Size = new Size(1126, 620);
            //_calendar.TabIndex = 0;

            //panel3.Controls.Add(_calendar);
        }
    }
}
