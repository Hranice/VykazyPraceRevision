using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Dialogs
{
    public partial class OverviewDialog : Form
    {
        private readonly User _user;
        private readonly DateRange _dateRange;
        private readonly ReportRepository _reportRepository;

        public OverviewDialog(
            User user,
            DateRange range,
            ReportRepository reportRepository)
        {
            InitializeComponent();

            _user = user;
            _dateRange = range;
            _reportRepository = reportRepository;

            Text = $"Přehled {range.FromDate:dd.MM.yyyy}-{range.ToDate:dd.MM.yyyy}";
        }

        private async void OverviewDialog_Load(object sender, EventArgs e)
        {
            try
            {
                var report = await _reportRepository.GetUserTimeReportAsync(
                    _user.Id,
                    _dateRange.FromDate,
                    _dateRange.ToDate);

                if (report == null)
                {
                    AppLogger.Error("Nepodařilo se načíst report uživatele.");
                    return;
                }

                labelReportedHours.Text = report.ReportedHours.ToString("0.##");
                labelActualHours.Text = report.ActualHours.ToString("0.##");

                var fund = await _reportRepository.GetHourFundAsync(
                    _user.Id,
                    _dateRange.FromDate,
                    _dateRange.ToDate);

                labelFund.Text = fund.ToString("0.##");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při načítání přehledu.", ex);
            }
        }
    }
}