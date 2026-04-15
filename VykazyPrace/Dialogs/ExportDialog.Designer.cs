namespace VykazyPrace.Dialogs
{
    partial class ExportDialog
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
            dtpFrom = new DateTimePicker();
            dtpTo = new DateTimePicker();
            label2 = new Label();
            bClose = new Button();
            cBMonth = new ComboBox();
            bLockEntries = new Button();
            gBLock = new GroupBox();
            label1 = new Label();
            gBExport = new GroupBox();
            tVUserGroupsUsers = new TreeView();
            panelSpecificYear = new Panel();
            bSetCurrentYear = new Button();
            rBSpecificYear = new RadioButton();
            nUDYear = new NumericUpDown();
            panelSpecificWeek = new Panel();
            bSetCurrentWeek = new Button();
            rBSpecificWeek = new RadioButton();
            nUDWeek = new NumericUpDown();
            panelSpecificMonth = new Panel();
            bSetCurrentMonth = new Button();
            rBSpecificMonth = new RadioButton();
            cBMonth2 = new ComboBox();
            panelSpecificTimePeriod = new Panel();
            rBSpecificTimePeriod = new RadioButton();
            bSaveAs = new Button();
            gBLock.SuspendLayout();
            gBExport.SuspendLayout();
            panelSpecificYear.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nUDYear).BeginInit();
            panelSpecificWeek.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nUDWeek).BeginInit();
            panelSpecificMonth.SuspendLayout();
            panelSpecificTimePeriod.SuspendLayout();
            SuspendLayout();
            // 
            // dtpFrom
            // 
            dtpFrom.Location = new Point(12, 40);
            dtpFrom.Margin = new Padding(4, 5, 4, 5);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Size = new Size(256, 28);
            dtpFrom.TabIndex = 2;
            dtpFrom.Value = new DateTime(2025, 3, 1, 15, 43, 0, 0);
            // 
            // dtpTo
            // 
            dtpTo.Location = new Point(286, 40);
            dtpTo.Margin = new Padding(4, 5, 4, 5);
            dtpTo.Name = "dtpTo";
            dtpTo.Size = new Size(256, 28);
            dtpTo.TabIndex = 4;
            dtpTo.Value = new DateTime(2025, 3, 31, 15, 44, 0, 0);
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(268, 38);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(18, 25);
            label2.TabIndex = 5;
            label2.Text = "-";
            // 
            // bClose
            // 
            bClose.DialogResult = DialogResult.Cancel;
            bClose.Location = new Point(567, 324);
            bClose.Name = "bClose";
            bClose.Size = new Size(109, 40);
            bClose.TabIndex = 8;
            bClose.Text = "Zavřít";
            bClose.UseVisualStyleBackColor = true;
            // 
            // cBMonth
            // 
            cBMonth.FormattingEnabled = true;
            cBMonth.Items.AddRange(new object[] { "Leden", "Únor", "Březen", "Duben", "Květen", "Červen", "Červenec", "Srpen", "Září", "Říjen", "Listopad", "Prosinec" });
            cBMonth.Location = new Point(15, 61);
            cBMonth.Name = "cBMonth";
            cBMonth.Size = new Size(81, 33);
            cBMonth.TabIndex = 11;
            cBMonth.Text = "Březen";
            cBMonth.SelectionChangeCommitted += cboMonth_SelectionChangeCommitted;
            // 
            // bLockEntries
            // 
            bLockEntries.Location = new Point(102, 61);
            bLockEntries.Name = "bLockEntries";
            bLockEntries.Size = new Size(219, 33);
            bLockEntries.TabIndex = 13;
            bLockEntries.Text = "ZAMKNOUT DATA";
            bLockEntries.UseVisualStyleBackColor = true;
            bLockEntries.Click += bLockEntries_Click;
            // 
            // gBLock
            // 
            gBLock.Controls.Add(label1);
            gBLock.Controls.Add(bLockEntries);
            gBLock.Controls.Add(cBMonth);
            gBLock.Location = new Point(12, 259);
            gBLock.Name = "gBLock";
            gBLock.Size = new Size(378, 107);
            gBLock.TabIndex = 14;
            gBLock.TabStop = false;
            gBLock.Text = "Zámek dat";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Reddit Sans", 10F);
            label1.Location = new Point(15, 36);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(346, 22);
            label1.TabIndex = 15;
            label1.Text = "Pro tento měsíc již nebude možné upravovat záznamy";
            // 
            // gBExport
            // 
            gBExport.Controls.Add(tVUserGroupsUsers);
            gBExport.Controls.Add(panelSpecificYear);
            gBExport.Controls.Add(panelSpecificWeek);
            gBExport.Controls.Add(panelSpecificMonth);
            gBExport.Controls.Add(panelSpecificTimePeriod);
            gBExport.Location = new Point(12, 12);
            gBExport.Name = "gBExport";
            gBExport.Size = new Size(865, 214);
            gBExport.TabIndex = 15;
            gBExport.TabStop = false;
            gBExport.Text = "Export dat";
            // 
            // tVUserGroupsUsers
            // 
            tVUserGroupsUsers.CheckBoxes = true;
            tVUserGroupsUsers.Location = new Point(578, 32);
            tVUserGroupsUsers.Name = "tVUserGroupsUsers";
            tVUserGroupsUsers.Size = new Size(273, 170);
            tVUserGroupsUsers.TabIndex = 27;
            tVUserGroupsUsers.AfterCheck += tVUserGroupsUsers_AfterCheck;
            // 
            // panelSpecificYear
            // 
            panelSpecificYear.BorderStyle = BorderStyle.FixedSingle;
            panelSpecificYear.Controls.Add(bSetCurrentYear);
            panelSpecificYear.Controls.Add(rBSpecificYear);
            panelSpecificYear.Controls.Add(nUDYear);
            panelSpecificYear.Location = new Point(373, 124);
            panelSpecificYear.Name = "panelSpecificYear";
            panelSpecificYear.Size = new Size(199, 78);
            panelSpecificYear.TabIndex = 26;
            panelSpecificYear.Click += panelTimePeriod_Click;
            // 
            // bSetCurrentYear
            // 
            bSetCurrentYear.Location = new Point(95, 38);
            bSetCurrentYear.Name = "bSetCurrentYear";
            bSetCurrentYear.Size = new Size(78, 35);
            bSetCurrentYear.TabIndex = 24;
            bSetCurrentYear.Text = "Dnešní";
            bSetCurrentYear.UseVisualStyleBackColor = true;
            bSetCurrentYear.Click += bSetCurrentYear_Click;
            // 
            // rBSpecificYear
            // 
            rBSpecificYear.AutoSize = true;
            rBSpecificYear.Location = new Point(12, 3);
            rBSpecificYear.Name = "rBSpecificYear";
            rBSpecificYear.Size = new Size(56, 29);
            rBSpecificYear.TabIndex = 22;
            rBSpecificYear.Text = "Rok";
            rBSpecificYear.UseVisualStyleBackColor = true;
            rBSpecificYear.Click += radioButtonTimePeriod_CheckedChanged;
            // 
            // nUDYear
            // 
            nUDYear.Font = new Font("Reddit Sans", 14F);
            nUDYear.Location = new Point(12, 39);
            nUDYear.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nUDYear.Minimum = new decimal(new int[] { 2000, 0, 0, 0 });
            nUDYear.Name = "nUDYear";
            nUDYear.Size = new Size(77, 32);
            nUDYear.TabIndex = 20;
            nUDYear.Value = new decimal(new int[] { 2026, 0, 0, 0 });
            // 
            // panelSpecificWeek
            // 
            panelSpecificWeek.BorderStyle = BorderStyle.FixedSingle;
            panelSpecificWeek.Controls.Add(bSetCurrentWeek);
            panelSpecificWeek.Controls.Add(rBSpecificWeek);
            panelSpecificWeek.Controls.Add(nUDWeek);
            panelSpecificWeek.Location = new Point(15, 124);
            panelSpecificWeek.Name = "panelSpecificWeek";
            panelSpecificWeek.Size = new Size(156, 78);
            panelSpecificWeek.TabIndex = 25;
            panelSpecificWeek.Click += panelTimePeriod_Click;
            // 
            // bSetCurrentWeek
            // 
            bSetCurrentWeek.Location = new Point(66, 36);
            bSetCurrentWeek.Name = "bSetCurrentWeek";
            bSetCurrentWeek.Size = new Size(78, 35);
            bSetCurrentWeek.TabIndex = 24;
            bSetCurrentWeek.Text = "Dnešní";
            bSetCurrentWeek.UseVisualStyleBackColor = true;
            bSetCurrentWeek.Click += bSetCurrentWeek_Click;
            // 
            // rBSpecificWeek
            // 
            rBSpecificWeek.AutoSize = true;
            rBSpecificWeek.Location = new Point(12, 3);
            rBSpecificWeek.Name = "rBSpecificWeek";
            rBSpecificWeek.Size = new Size(75, 29);
            rBSpecificWeek.TabIndex = 22;
            rBSpecificWeek.Text = "Týden";
            rBSpecificWeek.UseVisualStyleBackColor = true;
            rBSpecificWeek.Click += radioButtonTimePeriod_CheckedChanged;
            // 
            // nUDWeek
            // 
            nUDWeek.Font = new Font("Reddit Sans", 14F);
            nUDWeek.Location = new Point(12, 39);
            nUDWeek.Maximum = new decimal(new int[] { 53, 0, 0, 0 });
            nUDWeek.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nUDWeek.Name = "nUDWeek";
            nUDWeek.Size = new Size(48, 32);
            nUDWeek.TabIndex = 20;
            nUDWeek.Value = new decimal(new int[] { 52, 0, 0, 0 });
            // 
            // panelSpecificMonth
            // 
            panelSpecificMonth.BorderStyle = BorderStyle.FixedSingle;
            panelSpecificMonth.Controls.Add(bSetCurrentMonth);
            panelSpecificMonth.Controls.Add(rBSpecificMonth);
            panelSpecificMonth.Controls.Add(cBMonth2);
            panelSpecificMonth.Location = new Point(177, 124);
            panelSpecificMonth.Name = "panelSpecificMonth";
            panelSpecificMonth.Size = new Size(190, 78);
            panelSpecificMonth.TabIndex = 24;
            panelSpecificMonth.Click += panelTimePeriod_Click;
            // 
            // bSetCurrentMonth
            // 
            bSetCurrentMonth.Location = new Point(99, 38);
            bSetCurrentMonth.Name = "bSetCurrentMonth";
            bSetCurrentMonth.Size = new Size(78, 35);
            bSetCurrentMonth.TabIndex = 23;
            bSetCurrentMonth.Text = "Dnešní";
            bSetCurrentMonth.UseVisualStyleBackColor = true;
            bSetCurrentMonth.Click += bSetCurrentMonth_Click;
            // 
            // rBSpecificMonth
            // 
            rBSpecificMonth.AutoSize = true;
            rBSpecificMonth.Location = new Point(12, 3);
            rBSpecificMonth.Name = "rBSpecificMonth";
            rBSpecificMonth.Size = new Size(71, 29);
            rBSpecificMonth.TabIndex = 22;
            rBSpecificMonth.Text = "Měsíc";
            rBSpecificMonth.UseVisualStyleBackColor = true;
            rBSpecificMonth.Click += radioButtonTimePeriod_CheckedChanged;
            // 
            // cBMonth2
            // 
            cBMonth2.FormattingEnabled = true;
            cBMonth2.Items.AddRange(new object[] { "Leden", "Únor", "Březen", "Duben", "Květen", "Červen", "Červenec", "Srpen", "Září", "Říjen", "Listopad", "Prosinec" });
            cBMonth2.Location = new Point(12, 38);
            cBMonth2.Name = "cBMonth2";
            cBMonth2.Size = new Size(81, 33);
            cBMonth2.TabIndex = 19;
            cBMonth2.Text = "Březen";
            // 
            // panelSpecificTimePeriod
            // 
            panelSpecificTimePeriod.BackColor = SystemColors.ButtonHighlight;
            panelSpecificTimePeriod.BorderStyle = BorderStyle.FixedSingle;
            panelSpecificTimePeriod.Controls.Add(rBSpecificTimePeriod);
            panelSpecificTimePeriod.Controls.Add(label2);
            panelSpecificTimePeriod.Controls.Add(dtpTo);
            panelSpecificTimePeriod.Controls.Add(dtpFrom);
            panelSpecificTimePeriod.Location = new Point(15, 32);
            panelSpecificTimePeriod.Name = "panelSpecificTimePeriod";
            panelSpecificTimePeriod.Size = new Size(557, 86);
            panelSpecificTimePeriod.TabIndex = 23;
            panelSpecificTimePeriod.Click += panelTimePeriod_Click;
            // 
            // rBSpecificTimePeriod
            // 
            rBSpecificTimePeriod.AutoSize = true;
            rBSpecificTimePeriod.Checked = true;
            rBSpecificTimePeriod.Location = new Point(12, 3);
            rBSpecificTimePeriod.Name = "rBSpecificTimePeriod";
            rBSpecificTimePeriod.Size = new Size(188, 29);
            rBSpecificTimePeriod.TabIndex = 22;
            rBSpecificTimePeriod.TabStop = true;
            rBSpecificTimePeriod.Text = "Konkrétní časový úsek";
            rBSpecificTimePeriod.UseVisualStyleBackColor = true;
            rBSpecificTimePeriod.Click += radioButtonTimePeriod_CheckedChanged;
            // 
            // bSaveAs
            // 
            bSaveAs.Image = Properties.Resources.logo;
            bSaveAs.ImageAlign = ContentAlignment.MiddleLeft;
            bSaveAs.Location = new Point(682, 324);
            bSaveAs.Name = "bSaveAs";
            bSaveAs.Padding = new Padding(25, 0, 0, 0);
            bSaveAs.Size = new Size(181, 40);
            bSaveAs.TabIndex = 21;
            bSaveAs.Text = "Exportovat";
            bSaveAs.UseVisualStyleBackColor = true;
            bSaveAs.Click += bSaveAs_Click;
            // 
            // ExportDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(886, 376);
            Controls.Add(bSaveAs);
            Controls.Add(gBExport);
            Controls.Add(gBLock);
            Controls.Add(bClose);
            Font = new Font("Reddit Sans", 12F);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 5, 4, 5);
            Name = "ExportDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Exportovat data";
            Load += ExportDialog_Load;
            gBLock.ResumeLayout(false);
            gBLock.PerformLayout();
            gBExport.ResumeLayout(false);
            panelSpecificYear.ResumeLayout(false);
            panelSpecificYear.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nUDYear).EndInit();
            panelSpecificWeek.ResumeLayout(false);
            panelSpecificWeek.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nUDWeek).EndInit();
            panelSpecificMonth.ResumeLayout(false);
            panelSpecificMonth.PerformLayout();
            panelSpecificTimePeriod.ResumeLayout(false);
            panelSpecificTimePeriod.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Label label2;
        private Button bClose;
        private ComboBox cBMonth;
        private Button bLockEntries;
        private GroupBox gBLock;
        private Label label1;
        private GroupBox gBExport;
        private NumericUpDown nUDWeek;
        private ComboBox cBMonth2;
        private Button bSaveAs;
        private Panel panelSpecificWeek;
        private RadioButton rBSpecificWeek;
        private Panel panelSpecificMonth;
        private RadioButton rBSpecificMonth;
        private Panel panelSpecificTimePeriod;
        private RadioButton rBSpecificTimePeriod;
        private Button bSetCurrentWeek;
        private Button bSetCurrentMonth;
        private Panel panelSpecificYear;
        private Button bSetCurrentYear;
        private RadioButton rBSpecificYear;
        private NumericUpDown nUDYear;
        private TreeView tVUserGroupsUsers;
    }
}