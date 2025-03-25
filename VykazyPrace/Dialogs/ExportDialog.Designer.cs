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
            dateTimePicker1 = new DateTimePicker();
            dateTimePicker2 = new DateTimePicker();
            label2 = new Label();
            buttonSaveAs = new Button();
            dataGridView1 = new DataGridView();
            button2 = new Button();
            comboBoxMonth = new ComboBox();
            checkedListBoxUsers = new CheckedListBox();
            buttonLockEntries = new Button();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(10, 51);
            dateTimePicker1.Margin = new Padding(4, 5, 4, 5);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(256, 28);
            dateTimePicker1.TabIndex = 2;
            dateTimePicker1.Value = new DateTime(2025, 3, 1, 15, 43, 0, 0);
            dateTimePicker1.ValueChanged += DateTimePicker_ValueChanged;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Location = new Point(284, 51);
            dateTimePicker2.Margin = new Padding(4, 5, 4, 5);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(256, 28);
            dateTimePicker2.TabIndex = 4;
            dateTimePicker2.Value = new DateTime(2025, 3, 31, 15, 44, 0, 0);
            dateTimePicker2.ValueChanged += DateTimePicker_ValueChanged;
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
            // buttonSaveAs
            // 
            buttonSaveAs.Location = new Point(196, 453);
            buttonSaveAs.Name = "buttonSaveAs";
            buttonSaveAs.Size = new Size(344, 40);
            buttonSaveAs.TabIndex = 6;
            buttonSaveAs.Text = "Uložit jako..";
            buttonSaveAs.UseVisualStyleBackColor = true;
            buttonSaveAs.Click += ButtonSaveAs_Click;
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
            // button2
            // 
            button2.Location = new Point(10, 453);
            button2.Name = "button2";
            button2.Size = new Size(180, 40);
            button2.TabIndex = 8;
            button2.Text = "Zavřít";
            button2.UseVisualStyleBackColor = true;
            // 
            // comboBoxMonth
            // 
            comboBoxMonth.FormattingEnabled = true;
            comboBoxMonth.Items.AddRange(new object[] { "Leden", "Únor", "Březen", "Duben", "Květen", "Červen", "Červenec", "Srpen", "Září", "Říjen", "Listopad", "Prosinec" });
            comboBoxMonth.Location = new Point(10, 10);
            comboBoxMonth.Name = "comboBoxMonth";
            comboBoxMonth.Size = new Size(121, 33);
            comboBoxMonth.TabIndex = 11;
            comboBoxMonth.Text = "Březen";
            comboBoxMonth.SelectionChangeCommitted += ComboBoxMonth_SelectionChangeCommitted;
            // 
            // checkedListBoxUsers
            // 
            checkedListBoxUsers.Enabled = false;
            checkedListBoxUsers.FormattingEnabled = true;
            checkedListBoxUsers.Location = new Point(557, 90);
            checkedListBoxUsers.Name = "checkedListBoxUsers";
            checkedListBoxUsers.Size = new Size(309, 349);
            checkedListBoxUsers.TabIndex = 12;
            checkedListBoxUsers.SelectedValueChanged += checkedListBoxUsers_SelectedValueChanged;
            // 
            // buttonLockEntries
            // 
            buttonLockEntries.Location = new Point(137, 9);
            buttonLockEntries.Name = "buttonLockEntries";
            buttonLockEntries.Size = new Size(192, 33);
            buttonLockEntries.TabIndex = 13;
            buttonLockEntries.Text = "ZAMKNOUT DATA";
            buttonLockEntries.UseVisualStyleBackColor = true;
            buttonLockEntries.Click += buttonLockEntries_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.Red;
            label1.Location = new Point(681, 219);
            label1.Name = "label1";
            label1.Size = new Size(40, 25);
            label1.TabIndex = 14;
            label1.Text = "WIP";
            // 
            // ExportDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(881, 505);
            Controls.Add(label1);
            Controls.Add(buttonLockEntries);
            Controls.Add(checkedListBoxUsers);
            Controls.Add(comboBoxMonth);
            Controls.Add(button2);
            Controls.Add(dataGridView1);
            Controls.Add(buttonSaveAs);
            Controls.Add(label2);
            Controls.Add(dateTimePicker2);
            Controls.Add(dateTimePicker1);
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
        private DateTimePicker dateTimePicker1;
        private DateTimePicker dateTimePicker2;
        private Label label2;
        private Button buttonSaveAs;
        private DataGridView dataGridView1;
        private Button button2;
        private ComboBox comboBoxMonth;
        private CheckedListBox checkedListBoxUsers;
        private Button buttonLockEntries;
        private Label label1;
    }
}