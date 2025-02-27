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
            buttonProject = new Button();
            buttonContract = new Button();
            comboBoxProjectsContracts = new ComboBox();
            listBoxTimeEntries = new ListBox();
            groupBox1 = new GroupBox();
            label8 = new Label();
            label7 = new Label();
            textBoxDescription = new TextBox();
            labelHours = new Label();
            buttonAddHalfHour = new Button();
            buttonAddHour = new Button();
            buttonSubtractHalfHour = new Button();
            buttonSubtractHour = new Button();
            panel1 = new Panel();
            labelPreviousDate = new Label();
            labelFinishedHours = new Label();
            buttonRemove = new Button();
            buttonWrite = new Button();
            panel2 = new Panel();
            label6 = new Label();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // labelCurrentDate
            // 
            labelCurrentDate.Font = new Font("Reddit Sans", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelCurrentDate.Location = new Point(188, 9);
            labelCurrentDate.Name = "labelCurrentDate";
            labelCurrentDate.Size = new Size(150, 35);
            labelCurrentDate.TabIndex = 0;
            labelCurrentDate.Text = "25.02.2025";
            labelCurrentDate.TextAlign = ContentAlignment.TopCenter;
            // 
            // labelNextDate
            // 
            labelNextDate.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelNextDate.ForeColor = SystemColors.ControlDarkDark;
            labelNextDate.Location = new Point(344, 12);
            labelNextDate.Name = "labelNextDate";
            labelNextDate.Size = new Size(125, 25);
            labelNextDate.TabIndex = 2;
            labelNextDate.Text = "26.02.2025";
            labelNextDate.TextAlign = ContentAlignment.TopCenter;
            // 
            // buttonProject
            // 
            buttonProject.BackColor = Color.White;
            buttonProject.Dock = DockStyle.Left;
            buttonProject.FlatAppearance.BorderSize = 0;
            buttonProject.FlatStyle = FlatStyle.Flat;
            buttonProject.Font = new Font("Reddit Sans", 10F, FontStyle.Bold);
            buttonProject.Location = new Point(0, 0);
            buttonProject.Name = "buttonProject";
            buttonProject.Size = new Size(234, 31);
            buttonProject.TabIndex = 3;
            buttonProject.Text = "PROJEKT";
            buttonProject.UseVisualStyleBackColor = false;
            buttonProject.Click += buttonProject_Click;
            // 
            // buttonContract
            // 
            buttonContract.BackColor = SystemColors.AppWorkspace;
            buttonContract.Dock = DockStyle.Right;
            buttonContract.FlatAppearance.BorderSize = 0;
            buttonContract.FlatStyle = FlatStyle.Flat;
            buttonContract.Font = new Font("Reddit Sans", 10F);
            buttonContract.Location = new Point(232, 0);
            buttonContract.Name = "buttonContract";
            buttonContract.Size = new Size(234, 31);
            buttonContract.TabIndex = 4;
            buttonContract.Text = "ZAKÁZKA";
            buttonContract.UseVisualStyleBackColor = false;
            buttonContract.Click += buttonContract_Click;
            // 
            // comboBoxProjectsContracts
            // 
            comboBoxProjectsContracts.FormattingEnabled = true;
            comboBoxProjectsContracts.IntegralHeight = false;
            comboBoxProjectsContracts.ItemHeight = 25;
            comboBoxProjectsContracts.Location = new Point(10, 96);
            comboBoxProjectsContracts.Name = "comboBoxProjectsContracts";
            comboBoxProjectsContracts.Size = new Size(467, 33);
            comboBoxProjectsContracts.TabIndex = 5;
            comboBoxProjectsContracts.SelectionChangeCommitted += comboBoxProjectsContracts_SelectionChangeCommitted;
            comboBoxProjectsContracts.TextChanged += comboBoxProjectsContracts_TextChanged;
            // 
            // listBoxTimeEntries
            // 
            listBoxTimeEntries.BorderStyle = BorderStyle.FixedSingle;
            listBoxTimeEntries.FormattingEnabled = true;
            listBoxTimeEntries.ItemHeight = 25;
            listBoxTimeEntries.Location = new Point(12, 83);
            listBoxTimeEntries.Name = "listBoxTimeEntries";
            listBoxTimeEntries.Size = new Size(485, 77);
            listBoxTimeEntries.TabIndex = 9;
            listBoxTimeEntries.SelectedIndexChanged += listBoxTimeEntries_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(textBoxDescription);
            groupBox1.Controls.Add(labelHours);
            groupBox1.Controls.Add(buttonAddHalfHour);
            groupBox1.Controls.Add(buttonAddHour);
            groupBox1.Controls.Add(buttonSubtractHalfHour);
            groupBox1.Controls.Add(buttonSubtractHour);
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(comboBoxProjectsContracts);
            groupBox1.Location = new Point(12, 204);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(485, 299);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Zápis hodin";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label8.Location = new Point(7, 72);
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
            label7.Location = new Point(11, 132);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(55, 21);
            label7.TabIndex = 20;
            label7.Text = "Činnost";
            // 
            // textBoxDescription
            // 
            textBoxDescription.Location = new Point(11, 157);
            textBoxDescription.Margin = new Padding(5, 8, 5, 8);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.Size = new Size(466, 84);
            textBoxDescription.TabIndex = 19;
            // 
            // labelHours
            // 
            labelHours.Location = new Point(202, 245);
            labelHours.Name = "labelHours";
            labelHours.Size = new Size(84, 43);
            labelHours.TabIndex = 14;
            labelHours.Text = "0 h";
            labelHours.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // buttonAddHalfHour
            // 
            buttonAddHalfHour.Location = new Point(292, 245);
            buttonAddHalfHour.Name = "buttonAddHalfHour";
            buttonAddHalfHour.Size = new Size(90, 43);
            buttonAddHalfHour.TabIndex = 13;
            buttonAddHalfHour.Text = "+ 0,5 h";
            buttonAddHalfHour.UseVisualStyleBackColor = true;
            buttonAddHalfHour.Click += buttonAddHalfHour_Click;
            // 
            // buttonAddHour
            // 
            buttonAddHour.Location = new Point(388, 245);
            buttonAddHour.Name = "buttonAddHour";
            buttonAddHour.Size = new Size(90, 43);
            buttonAddHour.TabIndex = 12;
            buttonAddHour.Text = "+ 1 h";
            buttonAddHour.UseVisualStyleBackColor = true;
            buttonAddHour.Click += buttonAddHour_Click;
            // 
            // buttonSubtractHalfHour
            // 
            buttonSubtractHalfHour.Location = new Point(106, 245);
            buttonSubtractHalfHour.Name = "buttonSubtractHalfHour";
            buttonSubtractHalfHour.Size = new Size(90, 43);
            buttonSubtractHalfHour.TabIndex = 11;
            buttonSubtractHalfHour.Text = "- 0,5 h";
            buttonSubtractHalfHour.UseVisualStyleBackColor = true;
            buttonSubtractHalfHour.Click += buttonSubtractHalfHour_Click;
            // 
            // buttonSubtractHour
            // 
            buttonSubtractHour.Location = new Point(10, 245);
            buttonSubtractHour.Name = "buttonSubtractHour";
            buttonSubtractHour.Size = new Size(90, 43);
            buttonSubtractHour.TabIndex = 10;
            buttonSubtractHour.Text = "- 1 h";
            buttonSubtractHour.UseVisualStyleBackColor = true;
            buttonSubtractHour.Click += buttonSubtractHour_Click;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(buttonProject);
            panel1.Controls.Add(buttonContract);
            panel1.Location = new Point(11, 32);
            panel1.Name = "panel1";
            panel1.Size = new Size(468, 33);
            panel1.TabIndex = 9;
            // 
            // labelPreviousDate
            // 
            labelPreviousDate.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            labelPreviousDate.ForeColor = SystemColors.ControlDarkDark;
            labelPreviousDate.Location = new Point(57, 12);
            labelPreviousDate.Name = "labelPreviousDate";
            labelPreviousDate.Size = new Size(125, 25);
            labelPreviousDate.TabIndex = 11;
            labelPreviousDate.Text = "24.02.2025";
            labelPreviousDate.TextAlign = ContentAlignment.TopCenter;
            // 
            // labelFinishedHours
            // 
            labelFinishedHours.AutoSize = true;
            labelFinishedHours.Location = new Point(9, 168);
            labelFinishedHours.Name = "labelFinishedHours";
            labelFinishedHours.Size = new Size(73, 25);
            labelFinishedHours.TabIndex = 12;
            labelFinishedHours.Text = "7,5 / 15 h";
            // 
            // buttonRemove
            // 
            buttonRemove.Location = new Point(344, 509);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(153, 43);
            buttonRemove.TabIndex = 14;
            buttonRemove.Text = "Odstranit";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;
            // 
            // buttonWrite
            // 
            buttonWrite.Location = new Point(12, 509);
            buttonWrite.Name = "buttonWrite";
            buttonWrite.Size = new Size(326, 43);
            buttonWrite.TabIndex = 13;
            buttonWrite.Text = "Zapsat";
            buttonWrite.UseVisualStyleBackColor = true;
            buttonWrite.Click += buttonWrite_Click;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLight;
            panel2.Location = new Point(115, 48);
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
            ClientSize = new Size(509, 564);
            Controls.Add(label6);
            Controls.Add(panel2);
            Controls.Add(buttonRemove);
            Controls.Add(buttonWrite);
            Controls.Add(labelFinishedHours);
            Controls.Add(labelPreviousDate);
            Controls.Add(groupBox1);
            Controls.Add(listBoxTimeEntries);
            Controls.Add(labelNextDate);
            Controls.Add(labelCurrentDate);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntryDialog";
            Text = "Zápis hodin";
            Load += TimeEntryDialog_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelCurrentDate;
        private Label labelNextDate;
        private Button buttonProject;
        private Button buttonContract;
        private ComboBox comboBoxProjectsContracts;
        private ListBox listBoxTimeEntries;
        private GroupBox groupBox1;
        private Panel panel1;
        private Label labelPreviousDate;
        private Button buttonAddHalfHour;
        private Button buttonAddHour;
        private Button buttonSubtractHalfHour;
        private Button buttonSubtractHour;
        private Label labelFinishedHours;
        private Label labelHours;
        private Button buttonRemove;
        private Button buttonWrite;
        private Panel panel2;
        private Label label6;
        private Label label8;
        private Label label7;
        private TextBox textBoxDescription;
    }
}