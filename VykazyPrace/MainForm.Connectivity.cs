using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Connectivity;
using VykazyPrace.Infrastructure;

namespace VykazyPrace
{
    public partial class MainForm : Form
    {
        private ConnectivityService _connectivity;
        private ReadOnlyGuard _guard;
        private CancellationTokenSource _cts = new();
        private DateTime _lastNetworkEvent = DateTime.MinValue;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var sqlChecker = new SqlServerHealthChecker("Server=10.130.10.100;Database=powerkey;User Id=vykazprace;Password=!Vykaz2025!;TrustServerCertificate=True;");

            var config = ConfigService.Load();
            var sqliteChecker = new SqliteHealthChecker(config.DatabasePath);

            _connectivity = new ConnectivityService(sqlChecker, sqliteChecker);
            _connectivity.StatusChanged += Connectivity_StatusChanged;

            _guard = new ReadOnlyGuard(panelCalendarContainer, panelContainer);

            NetworkChange.NetworkAvailabilityChanged += (_, __) => DebouncedRefresh();
            NetworkChange.NetworkAddressChanged += (_, __) => DebouncedRefresh();

            _ = RefreshConnectivityAsync();
        }

        private void EnterCheckingState()
        {
            UiThread.SafeInvoke(this, () =>
            {
                SetButtonNeutral(buttonReloadPowerKey);
                SetButtonNeutral(buttonReloadNetworkDisks);

                _guard.SetReadOnly(true);
            });
        }

        private void DebouncedRefresh()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastNetworkEvent).TotalSeconds < 2) return; // jednoduchý debounce
            _lastNetworkEvent = now;
            _ = RefreshConnectivityAsync();
        }

        private async Task RefreshConnectivityAsync()
        {
            // Zrušíme případný probíhající check
            try { _cts.Cancel(); } catch { }
            _cts = new CancellationTokenSource();

            // Indikace „probíhá“
            EnterCheckingState();

            var snapshot = await _connectivity.CheckAllAsync(_cts.Token);
            ApplySnapshot(snapshot);
        }

        private void Connectivity_StatusChanged(object? sender, HealthSnapshot snapshot)
        {
            ApplySnapshot(snapshot);
        }

        private void ApplySnapshot(HealthSnapshot snapshot)
        {
            UiThread.SafeInvoke(this, () =>
            {
                SetButtonColor(buttonReloadPowerKey, snapshot.SqlServer);
                SetButtonColor(buttonReloadNetworkDisks, snapshot.Sqlite);
                _guard.SetReadOnly(snapshot.IsReadOnlyMode);
            });
        }

        private static void SetButtonColor(Button btn, ConnectionStatus status)
        {
            btn.BackColor = status switch
            {
                ConnectionStatus.Available => Color.LightGreen,
                ConnectionStatus.Unavailable => Color.LightCoral,
                _ => SystemColors.Control
            };
            btn.UseVisualStyleBackColor = false;
        }

        private static void SetButtonNeutral(Button btn)
        {
            btn.BackColor = Color.Khaki; // „kontroluji…“
            btn.UseVisualStyleBackColor = false;
        }

        private async void buttonReloadPowerKey_Click(object sender, EventArgs e)
        {
            await RefreshConnectivityAsync();
        }

        private async void buttonReloadNetworkDisks_Click(object sender, EventArgs e)
        {
            await RefreshConnectivityAsync();
        }
    }
}
