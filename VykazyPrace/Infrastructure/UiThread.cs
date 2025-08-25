namespace VykazyPrace.Infrastructure
{
    public static class UiThread
    {
        public static void SafeInvoke(Control control, Action action)
        {
            if (control.IsDisposed) return;
            if (control.InvokeRequired)
                control.BeginInvoke(action);
            else
                action();
        }
    }
}
