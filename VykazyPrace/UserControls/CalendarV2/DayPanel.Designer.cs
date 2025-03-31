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
            label2 = new Label();
            label1 = new Label();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Bottom;
            label2.Enabled = false;
            label2.Location = new Point(3, 175);
            label2.Name = "label2";
            label2.Size = new Size(14, 22);
            label2.TabIndex = 3;
            label2.Text = "!";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Enabled = false;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(14, 22);
            label1.TabIndex = 2;
            label1.Text = "!";
            // 
            // DayPanel
            // 
            AutoScaleDimensions = new SizeF(8F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 105, 87);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Reddit Sans", 10F);
            Margin = new Padding(3, 4, 3, 4);
            Name = "DayPanel";
            Padding = new Padding(3);
            Size = new Size(200, 200);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label2;
        private Label label1;
    }
}
