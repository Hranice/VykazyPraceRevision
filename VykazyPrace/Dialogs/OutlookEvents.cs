using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VykazyPrace.Dialogs
{
    public partial class OutlookEvents : Form
    {
        public OutlookEvents()
        {
            InitializeComponent();
        }

        private void InitializeEvents()
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
