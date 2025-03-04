namespace VykazyPrace.Dialogs
{
    partial class TimeEntryDialog
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
            labelCurrentDate = new Label();
            labelNextDate = new Label();
            comboBoxProjects = new ComboBox();
            listBoxTimeEntries = new ListBox();
            groupBox1 = new GroupBox();
            buttonRemove = new Button();
            buttonWrite = new Button();
            maskedTextBoxNumberOfHours = new MaskedTextBox();
            label2 = new Label();
            comboBoxEntryType = new ComboBox();
            label8 = new Label();
            label7 = new Label();
            textBoxDescription = new TextBox();
            labelPreviousDate = new Label();
            panel2 = new Panel();
            label6 = new Label();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // labelCurrentDate
            // 
            labelCurrentDate.Font = new Font("Reddit Sans", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelCurrentDate.Location = new Point(253, 9);
            labelCurrentDate.Name = "labelCurrentDate";
            labelCurrentDate.Size = new Size(150, 35);
            labelCurrentDate.TabIndex = 12;
            labelCurrentDate.Text = "25.02.2025";
            labelCurrentDate.TextAlign = ContentAlignment.TopCenter;
            labelCurrentDate.Click += labelCurrentDate_Click;
            // 
            // labelNextDate
            // 
            labelNextDate.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelNextDate.ForeColor = SystemColors.ControlDarkDark;
            labelNextDate.Location = new Point(409, 12);
            labelNextDate.Name = "labelNextDate";
            labelNextDate.Size = new Size(125, 25);
            labelNextDate.TabIndex = 13;
            labelNextDate.Text = "26.02.2025";
            labelNextDate.TextAlign = ContentAlignment.TopCenter;
            labelNextDate.Click += labelNextDate_Click;
            // 
            // comboBoxProjects
            // 
            comboBoxProjects.FormattingEnabled = true;
            comboBoxProjects.IntegralHeight = false;
            comboBoxProjects.ItemHeight = 25;
            comboBoxProjects.Location = new Point(136, 57);
            comboBoxProjects.Name = "comboBoxProjects";
            comboBoxProjects.Size = new Size(467, 33);
            comboBoxProjects.TabIndex = 3;
            comboBoxProjects.SelectionChangeCommitted += comboBoxProjectsContracts_SelectionChangeCommitted;
            comboBoxProjects.TextChanged += comboBoxProjectsContracts_TextChanged;
            // 
            // listBoxTimeEntries
            // 
            listBoxTimeEntries.BorderStyle = BorderStyle.FixedSingle;
            listBoxTimeEntries.FormattingEnabled = true;
            listBoxTimeEntries.ItemHeight = 25;
            listBoxTimeEntries.Location = new Point(12, 83);
            listBoxTimeEntries.Name = "listBoxTimeEntries";
            listBoxTimeEntries.Size = new Size(616, 102);
            listBoxTimeEntries.TabIndex = 9;
            listBoxTimeEntries.SelectedIndexChanged += listBoxTimeEntries_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(buttonRemove);
            groupBox1.Controls.Add(buttonWrite);
            groupBox1.Controls.Add(maskedTextBoxNumberOfHours);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(comboBoxEntryType);
            groupBox1.Controls.Add(comboBoxProjects);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(textBoxDescription);
            groupBox1.Location = new Point(12, 200);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(616, 263);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Zápis hodin (zbývá zapsat 7 h)";
            // 
            // buttonRemove
            // 
            buttonRemove.Location = new Point(10, 222);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(134, 32);
            buttonRemove.TabIndex = 29;
            buttonRemove.Text = "Odstranit";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;
            // 
            // buttonWrite
            // 
            buttonWrite.Location = new Point(472, 222);
            buttonWrite.Name = "buttonWrite";
            buttonWrite.Size = new Size(131, 32);
            buttonWrite.TabIndex = 28;
            buttonWrite.Text = "Zapsat";
            buttonWrite.UseVisualStyleBackColor = true;
            buttonWrite.Click += buttonWrite_Click;
            // 
            // maskedTextBoxNumberOfHours
            // 
            maskedTextBoxNumberOfHours.BorderStyle = BorderStyle.FixedSingle;
            maskedTextBoxNumberOfHours.Font = new Font("Reddit Sans", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            maskedTextBoxNumberOfHours.Location = new Point(402, 222);
            maskedTextBoxNumberOfHours.Mask = "0.0 h";
            maskedTextBoxNumberOfHours.Name = "maskedTextBoxNumberOfHours";
            maskedTextBoxNumberOfHours.Size = new Size(64, 32);
            maskedTextBoxNumberOfHours.TabIndex = 17;
            maskedTextBoxNumberOfHours.Text = "05";
            maskedTextBoxNumberOfHours.TextAlign = HorizontalAlignment.Center;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label2.Location = new Point(10, 33);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(31, 21);
            label2.TabIndex = 26;
            label2.Text = "Typ";
            // 
            // comboBoxEntryType
            // 
            comboBoxEntryType.FormattingEnabled = true;
            comboBoxEntryType.IntegralHeight = false;
            comboBoxEntryType.ItemHeight = 25;
            comboBoxEntryType.Items.AddRange(new object[] { "Obecné", "Administrativa", "Meeting", "Servis", "AfterCare" });
            comboBoxEntryType.Location = new Point(10, 57);
            comboBoxEntryType.Name = "comboBoxEntryType";
            comboBoxEntryType.Size = new Size(120, 33);
            comboBoxEntryType.TabIndex = 25;
            comboBoxEntryType.Text = "Obecné";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label8.Location = new Point(136, 33);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(57, 21);
            label8.TabIndex = 21;
            label8.Text = "Projekt*";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label7.Location = new Point(10, 102);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(60, 21);
            label7.TabIndex = 20;
            label7.Text = "Činnost*";
            // 
            // textBoxDescription
            // 
            textBoxDescription.Location = new Point(10, 127);
            textBoxDescription.Margin = new Padding(5, 8, 5, 8);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.Size = new Size(593, 84);
            textBoxDescription.TabIndex = 1;
            // 
            // labelPreviousDate
            // 
            labelPreviousDate.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelPreviousDate.ForeColor = SystemColors.ControlDarkDark;
            labelPreviousDate.Location = new Point(122, 12);
            labelPreviousDate.Name = "labelPreviousDate";
            labelPreviousDate.Size = new Size(125, 25);
            labelPreviousDate.TabIndex = 11;
            labelPreviousDate.Text = "24.02.2025";
            labelPreviousDate.TextAlign = ContentAlignment.TopCenter;
            labelPreviousDate.Click += labelPreviousDate_Click;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLight;
            panel2.Location = new Point(180, 48);
            panel2.Name = "panel2";
            panel2.Size = new Size(300, 1);
            panel2.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(9, 63);
            label6.Name = "label6";
            label6.Size = new Size(89, 17);
            label6.TabIndex = 16;
            label6.Text = "Zapsané hodiny:";
            // 
            // TimeEntryDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(640, 471);
            Controls.Add(label6);
            Controls.Add(panel2);
            Controls.Add(labelPreviousDate);
            Controls.Add(groupBox1);
            Controls.Add(listBoxTimeEntries);
            Controls.Add(labelNextDate);
            Controls.Add(labelCurrentDate);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntryDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Zápis hodin";
            Load += TimeEntryDialog_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelCurrentDate;
        private Label labelNextDate;
        private ComboBox comboBoxProjects;
        private ListBox listBoxTimeEntries;
        private GroupBox groupBox1;
        private Label labelPreviousDate;
        private Panel panel2;
        private Label label6;
        private Label label8;
        private Label label7;
        private TextBox textBoxDescription;
        private Label label2;
        private ComboBox comboBoxEntryType;
        private MaskedTextBox maskedTextBoxNumberOfHours;
        private Button buttonRemove;
        private Button buttonWrite;
    }
}