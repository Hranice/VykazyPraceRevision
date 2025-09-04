namespace VykazyPrace.Infrastructure
{
    /// <summary>
    /// Jednoduchý „guard“ – při výpadku spojení zakáže vybrané editační prvky.
    /// </summary>
    public sealed class ReadOnlyGuard
    {
        private readonly IReadOnlyList<Control> _controlsToDisable;

        public ReadOnlyGuard(params Control[] controlsToDisable)
        {
            _controlsToDisable = controlsToDisable;
        }

        public void SetReadOnly(bool readOnly)
        {
            foreach (var c in _controlsToDisable)
                c.Enabled = !readOnly;
        }
    }
}
