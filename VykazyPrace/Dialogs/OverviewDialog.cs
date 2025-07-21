using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;

namespace VykazyPrace.Dialogs
{
    public partial class OverviewDialog : Form
    {
        private User _user;
        private DateRange _dateRange;
        private ReportRepository _reportRepository = new();
        public OverviewDialog(User user, DateRange range)
        {
            InitializeComponent();
            _user = user;
            _dateRange = range;

            this.Text = $"Přehled {range.FromDate:dd.MM.yyyy}-{range.ToDate:dd.MM.yyyy}";
        }

        private async void OverviewDialog_Load(object sender, EventArgs e)
        {
            var report = await _reportRepository.GetUserTimeReportAsync(_user.Id, _dateRange.FromDate, _dateRange.ToDate);
            labelReportedHours.Text = report.ReportedHours.ToString();
            labelActualHours.Text = report.ActualHours.ToString();
            var fund = await _reportRepository.GetHourFundAsync(_user.Id, _dateRange.FromDate, _dateRange.ToDate);
            labelFund.Text = fund.ToString();
        }
    }
}
