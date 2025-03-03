namespace VykazyPrace.UserControls.Calendar
{
    partial class DayUC
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
            labelDay = new Label();
            labelHours = new Label();
            SuspendLayout();
            // 
            // labelDay
            // 
            labelDay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelDay.Font = new Font("Reddit Sans", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelDay.Location = new Point(43, 2);
            labelDay.Margin = new Padding(4, 0, 4, 0);
            labelDay.Name = "labelDay";
            labelDay.Size = new Size(43, 34);
            labelDay.TabIndex = 0;
            labelDay.Text = "00";
            labelDay.TextAlign = ContentAlignment.TopRight;
            // 
            // labelHours
            // 
            labelHours.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelHours.AutoSize = true;
            labelHours.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelHours.Location = new Point(5, 43);
            labelHours.Margin = new Padding(4, 0, 4, 0);
            labelHours.Name = "labelHours";
            labelHours.Size = new Size(0, 21);
            labelHours.TabIndex = 1;
            // 
            // DayUC
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(labelHours);
            Controls.Add(labelDay);
            Font = new Font("Reddit Sans", 12F);
            ForeColor = SystemColors.ControlDarkDark;
            Margin = new Padding(4, 5, 4, 5);
            Name = "DayUC";
            Size = new Size(90, 70);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public Label labelDay;
        public Label labelHours;
    }
}
