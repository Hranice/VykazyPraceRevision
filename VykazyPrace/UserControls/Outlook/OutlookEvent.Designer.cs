namespace VykazyPrace.UserControls.Outlook
{
    partial class OutlookEvent
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
            panel1 = new Panel();
            labelTime = new Label();
            labelDate = new Label();
            labelSubject = new Label();
            panel2 = new Panel();
            buttonAdd = new Button();
            buttonDelete = new Button();
            panel3 = new Panel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(83, 160, 222);
            panel1.Controls.Add(labelTime);
            panel1.Controls.Add(labelDate);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(621, 34);
            panel1.TabIndex = 3;
            // 
            // labelTime
            // 
            labelTime.AutoSize = true;
            labelTime.Dock = DockStyle.Right;
            labelTime.ForeColor = Color.White;
            labelTime.Location = new Point(522, 0);
            labelTime.Margin = new Padding(4, 0, 4, 0);
            labelTime.Name = "labelTime";
            labelTime.Padding = new Padding(0, 3, 0, 0);
            labelTime.Size = new Size(99, 28);
            labelTime.TabIndex = 4;
            labelTime.Text = "11:30 - 12:30";
            labelTime.TextAlign = ContentAlignment.TopRight;
            // 
            // labelDate
            // 
            labelDate.AutoSize = true;
            labelDate.Dock = DockStyle.Left;
            labelDate.ForeColor = Color.White;
            labelDate.Location = new Point(0, 0);
            labelDate.Margin = new Padding(4, 0, 4, 0);
            labelDate.Name = "labelDate";
            labelDate.Padding = new Padding(0, 3, 0, 0);
            labelDate.Size = new Size(88, 28);
            labelDate.TabIndex = 3;
            labelDate.Text = "23.10.2025";
            // 
            // labelSubject
            // 
            labelSubject.AutoSize = true;
            labelSubject.Dock = DockStyle.Top;
            labelSubject.Location = new Point(0, 34);
            labelSubject.MaximumSize = new Size(300, 1000);
            labelSubject.Name = "labelSubject";
            labelSubject.Padding = new Padding(3, 3, 0, 3);
            labelSubject.Size = new Size(285, 56);
            labelSubject.TabIndex = 6;
            labelSubject.Text = "Project meeting - pravidelný meeting [Osobně] ";
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(234, 246, 251);
            panel2.Controls.Add(buttonAdd);
            panel2.Controls.Add(buttonDelete);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 90);
            panel2.Name = "panel2";
            panel2.Padding = new Padding(3);
            panel2.Size = new Size(621, 43);
            panel2.TabIndex = 7;
            // 
            // buttonAdd
            // 
            buttonAdd.Dock = DockStyle.Fill;
            buttonAdd.Location = new Point(3, 3);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(463, 37);
            buttonAdd.TabIndex = 7;
            buttonAdd.Text = "Přidat";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.AutoSize = true;
            buttonDelete.Dock = DockStyle.Right;
            buttonDelete.ForeColor = Color.DarkRed;
            buttonDelete.Location = new Point(466, 3);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(152, 37);
            buttonDelete.TabIndex = 6;
            buttonDelete.Text = "Smazat";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += buttonDelete_Click;
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.Dock = DockStyle.Top;
            panel3.Location = new Point(0, 133);
            panel3.Name = "panel3";
            panel3.Size = new Size(621, 3);
            panel3.TabIndex = 8;
            // 
            // OutlookEvent
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(234, 246, 251);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(labelSubject);
            Controls.Add(panel1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(0);
            MinimumSize = new Size(267, 111);
            Name = "OutlookEvent";
            Size = new Size(621, 136);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel panel1;
        private Label labelTime;
        private Label labelDate;
        private Label labelSubject;
        private Panel panel2;
        private Button buttonDelete;
        private Panel panel3;
        private Button buttonAdd;
    }
}
