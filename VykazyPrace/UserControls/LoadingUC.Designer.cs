namespace VykazyPrace.UserControls
{
    partial class LoadingUC
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
            label1 = new Label();
            progressBar1 = new ProgressBar();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 10);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(113, 25);
            label1.TabIndex = 15;
            label1.Text = "Načítám data..";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(10, 40);
            progressBar1.Margin = new Padding(4, 5, 4, 5);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(250, 32);
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.TabIndex = 14;
            // 
            // LoadingUC
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(label1);
            Controls.Add(progressBar1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "LoadingUC";
            Size = new Size(270, 84);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ProgressBar progressBar1;
    }
}
