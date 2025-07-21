namespace VykazyPrace.Dialogs
{
    partial class OverviewDialog
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
            groupBox2 = new GroupBox();
            labelFund = new Label();
            label5 = new Label();
            labelActualHours = new Label();
            label3 = new Label();
            labelReportedHours = new Label();
            label1 = new Label();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(labelFund);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(labelActualHours);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(labelReportedHours);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(350, 167);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Přehled vykázaných a odpracovaných hodin";
            // 
            // labelFund
            // 
            labelFund.AutoSize = true;
            labelFund.Location = new Point(110, 121);
            labelFund.Name = "labelFund";
            labelFund.Size = new Size(35, 25);
            labelFund.TabIndex = 8;
            labelFund.Text = "0 h";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(18, 121);
            label5.Name = "label5";
            label5.Size = new Size(51, 25);
            label5.TabIndex = 7;
            label5.Text = "Fond:";
            // 
            // labelActualHours
            // 
            labelActualHours.AutoSize = true;
            labelActualHours.Location = new Point(110, 73);
            labelActualHours.Name = "labelActualHours";
            labelActualHours.Size = new Size(35, 25);
            labelActualHours.TabIndex = 6;
            labelActualHours.Text = "0 h";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(18, 73);
            label3.Name = "label3";
            label3.Size = new Size(85, 25);
            label3.TabIndex = 5;
            label3.Text = "Docházka:";
            // 
            // labelReportedHours
            // 
            labelReportedHours.AutoSize = true;
            labelReportedHours.Location = new Point(110, 38);
            labelReportedHours.Name = "labelReportedHours";
            labelReportedHours.Size = new Size(35, 25);
            labelReportedHours.TabIndex = 4;
            labelReportedHours.Text = "0 h";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 38);
            label1.Name = "label1";
            label1.Size = new Size(86, 25);
            label1.TabIndex = 3;
            label1.Text = "Vykázáno:";
            // 
            // OverviewDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(370, 187);
            Controls.Add(groupBox2);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "OverviewDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Přehled";
            Load += OverviewDialog_Load;
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox2;
        private Button buttonPathToDatabase;
        private Label label1;
        private Label labelReportedHours;
        private Label labelFund;
        private Label label5;
        private Label labelActualHours;
        private Label label3;
    }
}