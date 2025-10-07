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
            btnSaveAs = new Button();
            dataGridView1 = new DataGridView();
            btnClose = new Button();
            cboMonth = new ComboBox();
            clbUserGroups = new CheckedListBox();
            btnLockEntries = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dtpFrom
            // 
            dtpFrom.Location = new Point(10, 51);
            dtpFrom.Margin = new Padding(4, 5, 4, 5);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Size = new Size(256, 28);
            dtpFrom.TabIndex = 2;
            dtpFrom.Value = new DateTime(2025, 3, 1, 15, 43, 0, 0);
            // 
            // dtpTo
            // 
            dtpTo.Location = new Point(284, 51);
            dtpTo.Margin = new Padding(4, 5, 4, 5);
            dtpTo.Name = "dtpTo";
            dtpTo.Size = new Size(256, 28);
            dtpTo.TabIndex = 4;
            dtpTo.Value = new DateTime(2025, 3, 31, 15, 44, 0, 0);
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(266, 49);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(18, 25);
            label2.TabIndex = 5;
            label2.Text = "-";
            // 
            // btnSaveAs
            // 
            btnSaveAs.Location = new Point(196, 453);
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Size = new Size(344, 40);
            btnSaveAs.TabIndex = 6;
            btnSaveAs.Text = "Uložit jako..";
            btnSaveAs.UseVisualStyleBackColor = true;
            btnSaveAs.Click += btnSaveAs_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.Location = new Point(10, 90);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(530, 349);
            dataGridView1.TabIndex = 7;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(10, 453);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(180, 40);
            btnClose.TabIndex = 8;
            btnClose.Text = "Zavřít";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // cboMonth
            // 
            cboMonth.FormattingEnabled = true;
            cboMonth.Items.AddRange(new object[] { "Leden", "Únor", "Březen", "Duben", "Květen", "Červen", "Červenec", "Srpen", "Září", "Říjen", "Listopad", "Prosinec" });
            cboMonth.Location = new Point(10, 10);
            cboMonth.Name = "cboMonth";
            cboMonth.Size = new Size(121, 33);
            cboMonth.TabIndex = 11;
            cboMonth.Text = "Březen";
            cboMonth.SelectionChangeCommitted += cboMonth_SelectionChangeCommitted;
            // 
            // clbUserGroups
            // 
            clbUserGroups.FormattingEnabled = true;
            clbUserGroups.Location = new Point(557, 90);
            clbUserGroups.Name = "clbUserGroups";
            clbUserGroups.Size = new Size(309, 349);
            clbUserGroups.TabIndex = 12;
            // 
            // btnLockEntries
            // 
            btnLockEntries.Location = new Point(137, 9);
            btnLockEntries.Name = "btnLockEntries";
            btnLockEntries.Size = new Size(192, 33);
            btnLockEntries.TabIndex = 13;
            btnLockEntries.Text = "ZAMKNOUT DATA";
            btnLockEntries.UseVisualStyleBackColor = true;
            btnLockEntries.Click += btnLockEntries_Click;
            // 
            // ExportDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(881, 505);
            Controls.Add(btnLockEntries);
            Controls.Add(clbUserGroups);
            Controls.Add(cboMonth);
            Controls.Add(btnClose);
            Controls.Add(dataGridView1);
            Controls.Add(btnSaveAs);
            Controls.Add(label2);
            Controls.Add(dtpTo);
            Controls.Add(dtpFrom);
            Font = new Font("Reddit Sans", 12F);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4, 5, 4, 5);
            Name = "ExportDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Exportovat data";
            Load += ExportDialog_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Label label2;
        private Button btnSaveAs;
        private DataGridView dataGridView1;
        private Button btnClose;
        private ComboBox cboMonth;
        private CheckedListBox clbUserGroups;
        private Button btnLockEntries;
    }
}