namespace VykazyPrace.UserControls.CalendarV2
{
    partial class DayPanel
    {
        /// <summary> 
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód vygenerovaný pomocí Návrháře komponent

        /// <summary> 
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // DayPanel
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(255, 105, 87);
            Font = new Font("Arial Narrow", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            Margin = new Padding(3, 4, 3, 4);
            Name = "DayPanel";
            Padding = new Padding(3);
            Size = new Size(200, 200);
            ResumeLayout(false);
        }

        #endregion
    }
}
