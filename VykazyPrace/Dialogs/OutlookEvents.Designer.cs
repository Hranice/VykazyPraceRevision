namespace VykazyPrace.Dialogs
{
    partial class OutlookEvents
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            outlookEvent4 = new VykazyPrace.UserControls.Outlook.OutlookEvent();
            outlookEvent3 = new VykazyPrace.UserControls.Outlook.OutlookEvent();
            outlookEvent2 = new VykazyPrace.UserControls.Outlook.OutlookEvent();
            outlookEvent1 = new VykazyPrace.UserControls.Outlook.OutlookEvent();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoScroll = true;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(outlookEvent4, 0, 3);
            tableLayoutPanel1.Controls.Add(outlookEvent3, 0, 2);
            tableLayoutPanel1.Controls.Add(outlookEvent2, 0, 0);
            tableLayoutPanel1.Controls.Add(outlookEvent1, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(245, 437);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // outlookEvent4
            // 
            outlookEvent4.AutoSize = true;
            outlookEvent4.BackColor = Color.White;
            outlookEvent4.Font = new Font("Reddit Sans", 12F);
            outlookEvent4.Location = new Point(4, 437);
            outlookEvent4.Margin = new Padding(4, 5, 4, 5);
            outlookEvent4.MinimumSize = new Size(220, 0);
            outlookEvent4.Name = "outlookEvent4";
            outlookEvent4.Padding = new Padding(2);
            outlookEvent4.Size = new Size(220, 134);
            outlookEvent4.TabIndex = 3;
            // 
            // outlookEvent3
            // 
            outlookEvent3.AutoSize = true;
            outlookEvent3.BackColor = Color.White;
            outlookEvent3.Font = new Font("Reddit Sans", 12F);
            outlookEvent3.Location = new Point(4, 293);
            outlookEvent3.Margin = new Padding(4, 5, 4, 5);
            outlookEvent3.MinimumSize = new Size(220, 0);
            outlookEvent3.Name = "outlookEvent3";
            outlookEvent3.Padding = new Padding(2);
            outlookEvent3.Size = new Size(220, 134);
            outlookEvent3.TabIndex = 2;
            // 
            // outlookEvent2
            // 
            outlookEvent2.AutoSize = true;
            outlookEvent2.BackColor = Color.White;
            outlookEvent2.Font = new Font("Reddit Sans", 12F);
            outlookEvent2.Location = new Point(4, 5);
            outlookEvent2.Margin = new Padding(4, 5, 4, 5);
            outlookEvent2.MinimumSize = new Size(220, 0);
            outlookEvent2.Name = "outlookEvent2";
            outlookEvent2.Padding = new Padding(2);
            outlookEvent2.Size = new Size(220, 134);
            outlookEvent2.TabIndex = 1;
            // 
            // outlookEvent1
            // 
            outlookEvent1.AutoSize = true;
            outlookEvent1.BackColor = Color.White;
            outlookEvent1.Font = new Font("Reddit Sans", 12F);
            outlookEvent1.Location = new Point(4, 149);
            outlookEvent1.Margin = new Padding(4, 5, 4, 5);
            outlookEvent1.MinimumSize = new Size(220, 0);
            outlookEvent1.Name = "outlookEvent1";
            outlookEvent1.Padding = new Padding(2);
            outlookEvent1.Size = new Size(220, 134);
            outlookEvent1.TabIndex = 0;
            // 
            // OutlookEvents
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(245, 437);
            Controls.Add(tableLayoutPanel1);
            Name = "OutlookEvents";
            Text = "Události v Outlooku";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel tableLayoutPanel1;
        private UserControls.Outlook.OutlookEvent outlookEvent2;
        private UserControls.Outlook.OutlookEvent outlookEvent1;
        private UserControls.Outlook.OutlookEvent outlookEvent4;
        private UserControls.Outlook.OutlookEvent outlookEvent3;
    }
}