namespace VykazyPrace.UserControls.Calendar
{
    partial class CalendarUC
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
            panelContainer = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            labelNextMonth = new Label();
            labelMonth = new Label();
            labelPreviousMonth = new Label();
            panelContainer.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panelContainer
            // 
            panelContainer.AutoScroll = true;
            panelContainer.Controls.Add(tableLayoutPanel1);
            panelContainer.Location = new Point(0, 45);
            panelContainer.Margin = new Padding(4, 5, 4, 5);
            panelContainer.Name = "panelContainer";
            panelContainer.Size = new Size(658, 514);
            panelContainer.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 7;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26F));
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Enabled = false;
            tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 5, 4, 5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.Size = new Size(658, 514);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // panel1
            // 
            panel1.Controls.Add(labelNextMonth);
            panel1.Controls.Add(labelMonth);
            panel1.Controls.Add(labelPreviousMonth);
            panel1.Dock = DockStyle.Top;
            panel1.Font = new Font("Reddit Sans", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(10, 10, 0, 0);
            panel1.Size = new Size(658, 45);
            panel1.TabIndex = 4;
            // 
            // labelNextMonth
            // 
            labelNextMonth.AutoSize = true;
            labelNextMonth.Dock = DockStyle.Left;
            labelNextMonth.Location = new Point(200, 10);
            labelNextMonth.Margin = new Padding(4, 0, 4, 0);
            labelNextMonth.Name = "labelNextMonth";
            labelNextMonth.Padding = new Padding(0, 0, 20, 0);
            labelNextMonth.Size = new Size(40, 25);
            labelNextMonth.TabIndex = 5;
            labelNextMonth.Text = ">";
            labelNextMonth.Click += labelNextMonth_Click;
            // 
            // labelMonth
            // 
            labelMonth.Dock = DockStyle.Left;
            labelMonth.Location = new Point(50, 10);
            labelMonth.Margin = new Padding(4, 0, 4, 0);
            labelMonth.Name = "labelMonth";
            labelMonth.Padding = new Padding(0, 0, 20, 0);
            labelMonth.Size = new Size(150, 35);
            labelMonth.TabIndex = 4;
            labelMonth.Text = "labelMonth";
            labelMonth.TextAlign = ContentAlignment.TopCenter;
            // 
            // labelPreviousMonth
            // 
            labelPreviousMonth.AutoSize = true;
            labelPreviousMonth.Dock = DockStyle.Left;
            labelPreviousMonth.Location = new Point(10, 10);
            labelPreviousMonth.Margin = new Padding(4, 0, 4, 0);
            labelPreviousMonth.Name = "labelPreviousMonth";
            labelPreviousMonth.Padding = new Padding(0, 0, 20, 0);
            labelPreviousMonth.Size = new Size(40, 25);
            labelPreviousMonth.TabIndex = 2;
            labelPreviousMonth.Text = "<";
            labelPreviousMonth.Click += labelPreviousMonth_Click;
            // 
            // CalendarUC
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(panelContainer);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "CalendarUC";
            Size = new Size(658, 563);
            Load += CalendarUC_Load;
            panelContainer.ResumeLayout(false);
            panelContainer.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panelContainer;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Label labelMonth;
        private Label labelPreviousMonth;
        private Label labelNextMonth;
    }
}
