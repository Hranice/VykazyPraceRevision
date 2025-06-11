namespace VykazyPrace.Dialogs
{
    partial class ProjectManagementDialog
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
            label6 = new Label();
            listBoxProject = new ListBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            label4 = new Label();
            buttonAddOperation = new Button();
            listBoxOperation = new ListBox();
            label10 = new Label();
            textBoxOperation = new TextBox();
            tabPage2 = new TabPage();
            groupBox2 = new GroupBox();
            buttonDeclineAndReplace = new Button();
            labelProjectId = new Label();
            label9 = new Label();
            textBoxProjectTitle = new TextBox();
            buttonArchiveProject = new Button();
            buttonAddProject = new Button();
            textBoxProjectDescription = new TextBox();
            label7 = new Label();
            groupBox1 = new GroupBox();
            checkBoxProposed = new CheckBox();
            checkBoxArchived = new CheckBox();
            comboBoxProjects = new ComboBox();
            label14 = new Label();
            tabPage3 = new TabPage();
            label8 = new Label();
            buttonAddAbsence = new Button();
            listBoxAbsence = new ListBox();
            label11 = new Label();
            textBoxAbsence = new TextBox();
            tabPage4 = new TabPage();
            label12 = new Label();
            buttonAddOther = new Button();
            listBoxOther = new ListBox();
            label13 = new Label();
            textBoxOther = new TextBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(401, 12);
            label6.Name = "label6";
            label6.Size = new Size(93, 17);
            label6.TabIndex = 24;
            label6.Text = "Seznam projektů:";
            // 
            // listBoxProject
            // 
            listBoxProject.BorderStyle = BorderStyle.FixedSingle;
            listBoxProject.FormattingEnabled = true;
            listBoxProject.ItemHeight = 25;
            listBoxProject.Location = new Point(14, 25);
            listBoxProject.Name = "listBoxProject";
            listBoxProject.Size = new Size(476, 202);
            listBoxProject.TabIndex = 23;
            listBoxProject.SelectedIndexChanged += listBoxProject_SelectedIndexChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(512, 532);
            tabControl1.TabIndex = 31;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(buttonAddOperation);
            tabPage1.Controls.Add(listBoxOperation);
            tabPage1.Controls.Add(label10);
            tabPage1.Controls.Add(textBoxOperation);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(504, 494);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "PROVOZ";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label4.Location = new Point(14, 3);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(55, 21);
            label4.TabIndex = 51;
            label4.Text = "Seznam";
            // 
            // buttonAddOperation
            // 
            buttonAddOperation.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAddOperation.Location = new Point(369, 445);
            buttonAddOperation.Margin = new Padding(4, 5, 4, 5);
            buttonAddOperation.Name = "buttonAddOperation";
            buttonAddOperation.Size = new Size(121, 36);
            buttonAddOperation.TabIndex = 50;
            buttonAddOperation.Text = "Přidat";
            buttonAddOperation.UseVisualStyleBackColor = true;
            buttonAddOperation.Click += buttonAddOperation_Click;
            // 
            // listBoxOperation
            // 
            listBoxOperation.BorderStyle = BorderStyle.FixedSingle;
            listBoxOperation.FormattingEnabled = true;
            listBoxOperation.ItemHeight = 25;
            listBoxOperation.Location = new Point(14, 25);
            listBoxOperation.Name = "listBoxOperation";
            listBoxOperation.Size = new Size(476, 327);
            listBoxOperation.TabIndex = 45;
            listBoxOperation.SelectedIndexChanged += listBoxOperation_SelectedIndexChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label10.Location = new Point(14, 379);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(134, 21);
            label10.TabIndex = 47;
            label10.Text = "Nákladové středisko*";
            // 
            // textBoxOperation
            // 
            textBoxOperation.Location = new Point(14, 404);
            textBoxOperation.Margin = new Padding(5, 8, 5, 8);
            textBoxOperation.Name = "textBoxOperation";
            textBoxOperation.PlaceholderText = "Provoz HP - Horká ražba";
            textBoxOperation.Size = new Size(476, 28);
            textBoxOperation.TabIndex = 46;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(groupBox2);
            tabPage2.Controls.Add(groupBox1);
            tabPage2.Controls.Add(label14);
            tabPage2.Controls.Add(listBoxProject);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(504, 494);
            tabPage2.TabIndex = 2;
            tabPage2.Text = "PROJEKT";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(buttonDeclineAndReplace);
            groupBox2.Controls.Add(labelProjectId);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(textBoxProjectTitle);
            groupBox2.Controls.Add(buttonArchiveProject);
            groupBox2.Controls.Add(buttonAddProject);
            groupBox2.Controls.Add(textBoxProjectDescription);
            groupBox2.Controls.Add(label7);
            groupBox2.Font = new Font("Reddit Sans", 9.75F);
            groupBox2.Location = new Point(14, 357);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(476, 125);
            groupBox2.TabIndex = 59;
            groupBox2.TabStop = false;
            groupBox2.Text = "Nový projekt";
            // 
            // buttonDeclineAndReplace
            // 
            buttonDeclineAndReplace.Font = new Font("Reddit Sans", 12F);
            buttonDeclineAndReplace.Location = new Point(153, 82);
            buttonDeclineAndReplace.Margin = new Padding(4, 5, 4, 5);
            buttonDeclineAndReplace.Name = "buttonDeclineAndReplace";
            buttonDeclineAndReplace.Size = new Size(187, 36);
            buttonDeclineAndReplace.TabIndex = 58;
            buttonDeclineAndReplace.Text = "Zamítnout a nahradit";
            buttonDeclineAndReplace.UseVisualStyleBackColor = true;
            buttonDeclineAndReplace.Visible = false;
            buttonDeclineAndReplace.Click += buttonDeclineAndReplace_Click;
            // 
            // labelProjectId
            // 
            labelProjectId.AutoSize = true;
            labelProjectId.Location = new Point(12, 91);
            labelProjectId.Margin = new Padding(4, 0, 4, 0);
            labelProjectId.Name = "labelProjectId";
            labelProjectId.Size = new Size(0, 21);
            labelProjectId.TabIndex = 57;
            labelProjectId.Visible = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(12, 28);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(121, 21);
            label9.TabIndex = 38;
            label9.Text = "Označení projektu*";
            // 
            // textBoxProjectTitle
            // 
            textBoxProjectTitle.Font = new Font("Reddit Sans", 12F);
            textBoxProjectTitle.Location = new Point(167, 50);
            textBoxProjectTitle.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectTitle.Name = "textBoxProjectTitle";
            textBoxProjectTitle.PlaceholderText = "Projekt vývoje software na výkaz hodin";
            textBoxProjectTitle.Size = new Size(297, 28);
            textBoxProjectTitle.TabIndex = 37;
            // 
            // buttonArchiveProject
            // 
            buttonArchiveProject.Font = new Font("Reddit Sans", 12F);
            buttonArchiveProject.Location = new Point(219, 82);
            buttonArchiveProject.Margin = new Padding(4, 5, 4, 5);
            buttonArchiveProject.Name = "buttonArchiveProject";
            buttonArchiveProject.Size = new Size(121, 36);
            buttonArchiveProject.TabIndex = 56;
            buttonArchiveProject.Text = "Archivovat";
            buttonArchiveProject.UseVisualStyleBackColor = true;
            buttonArchiveProject.Visible = false;
            buttonArchiveProject.Click += buttonArchiveProject_Click;
            // 
            // buttonAddProject
            // 
            buttonAddProject.Font = new Font("Reddit Sans", 12F);
            buttonAddProject.Location = new Point(343, 82);
            buttonAddProject.Margin = new Padding(4, 5, 4, 5);
            buttonAddProject.Name = "buttonAddProject";
            buttonAddProject.Size = new Size(121, 36);
            buttonAddProject.TabIndex = 41;
            buttonAddProject.Text = "Přidat";
            buttonAddProject.UseVisualStyleBackColor = true;
            buttonAddProject.Click += buttonAddProject_Click;
            // 
            // textBoxProjectDescription
            // 
            textBoxProjectDescription.Font = new Font("Reddit Sans", 12F);
            textBoxProjectDescription.Location = new Point(12, 50);
            textBoxProjectDescription.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectDescription.Name = "textBoxProjectDescription";
            textBoxProjectDescription.PlaceholderText = "000E00";
            textBoxProjectDescription.Size = new Size(135, 28);
            textBoxProjectDescription.TabIndex = 39;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(167, 28);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(104, 21);
            label7.TabIndex = 40;
            label7.Text = "Název projektu*";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBoxProposed);
            groupBox1.Controls.Add(checkBoxArchived);
            groupBox1.Controls.Add(comboBoxProjects);
            groupBox1.Font = new Font("Reddit Sans", 9.75F);
            groupBox1.Location = new Point(14, 233);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(476, 118);
            groupBox1.TabIndex = 58;
            groupBox1.TabStop = false;
            groupBox1.Text = "Filtr";
            // 
            // checkBoxProposed
            // 
            checkBoxProposed.AutoSize = true;
            checkBoxProposed.Font = new Font("Reddit Sans", 12F);
            checkBoxProposed.Location = new Point(153, 31);
            checkBoxProposed.Name = "checkBoxProposed";
            checkBoxProposed.Size = new Size(85, 29);
            checkBoxProposed.TabIndex = 58;
            checkBoxProposed.Text = "NÁVRH";
            checkBoxProposed.UseVisualStyleBackColor = true;
            // 
            // checkBoxArchived
            // 
            checkBoxArchived.AutoSize = true;
            checkBoxArchived.Font = new Font("Reddit Sans", 12F);
            checkBoxArchived.Location = new Point(12, 31);
            checkBoxArchived.Name = "checkBoxArchived";
            checkBoxArchived.Size = new Size(135, 29);
            checkBoxArchived.TabIndex = 57;
            checkBoxArchived.Text = "ARCHIVOVÁN";
            checkBoxArchived.UseVisualStyleBackColor = true;
            checkBoxArchived.CheckedChanged += checkBoxArchive_CheckedChanged;
            // 
            // comboBoxProjects
            // 
            comboBoxProjects.Font = new Font("Reddit Sans", 12F);
            comboBoxProjects.FormattingEnabled = true;
            comboBoxProjects.Location = new Point(12, 66);
            comboBoxProjects.Name = "comboBoxProjects";
            comboBoxProjects.Size = new Size(452, 33);
            comboBoxProjects.TabIndex = 43;
            comboBoxProjects.SelectionChangeCommitted += comboBoxProjects_SelectionChangeCommitted;
            comboBoxProjects.TextChanged += comboBoxProjects_TextChanged;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label14.Location = new Point(14, 3);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new Size(55, 21);
            label14.TabIndex = 52;
            label14.Text = "Seznam";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(label8);
            tabPage3.Controls.Add(buttonAddAbsence);
            tabPage3.Controls.Add(listBoxAbsence);
            tabPage3.Controls.Add(label11);
            tabPage3.Controls.Add(textBoxAbsence);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(504, 504);
            tabPage3.TabIndex = 3;
            tabPage3.Text = "NEPŘÍTOMNOST";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label8.Location = new Point(14, 3);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(55, 21);
            label8.TabIndex = 58;
            label8.Text = "Seznam";
            // 
            // buttonAddAbsence
            // 
            buttonAddAbsence.Location = new Point(369, 445);
            buttonAddAbsence.Margin = new Padding(4, 5, 4, 5);
            buttonAddAbsence.Name = "buttonAddAbsence";
            buttonAddAbsence.Size = new Size(121, 36);
            buttonAddAbsence.TabIndex = 57;
            buttonAddAbsence.Text = "Přidat";
            buttonAddAbsence.UseVisualStyleBackColor = true;
            buttonAddAbsence.Click += buttonAddAbsence_Click;
            // 
            // listBoxAbsence
            // 
            listBoxAbsence.BorderStyle = BorderStyle.FixedSingle;
            listBoxAbsence.FormattingEnabled = true;
            listBoxAbsence.ItemHeight = 25;
            listBoxAbsence.Location = new Point(14, 25);
            listBoxAbsence.Name = "listBoxAbsence";
            listBoxAbsence.Size = new Size(476, 327);
            listBoxAbsence.TabIndex = 54;
            listBoxAbsence.SelectedIndexChanged += listBoxAbsence_SelectedIndexChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label11.Location = new Point(14, 379);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(115, 21);
            label11.TabIndex = 56;
            label11.Text = "Typ nepřítomosti*";
            // 
            // textBoxAbsence
            // 
            textBoxAbsence.Location = new Point(14, 404);
            textBoxAbsence.Margin = new Padding(5, 8, 5, 8);
            textBoxAbsence.Name = "textBoxAbsence";
            textBoxAbsence.PlaceholderText = "Lékař";
            textBoxAbsence.Size = new Size(476, 28);
            textBoxAbsence.TabIndex = 55;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(label12);
            tabPage4.Controls.Add(buttonAddOther);
            tabPage4.Controls.Add(listBoxOther);
            tabPage4.Controls.Add(label13);
            tabPage4.Controls.Add(textBoxOther);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(504, 504);
            tabPage4.TabIndex = 4;
            tabPage4.Text = "OSTATNÍ";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label12.Location = new Point(14, 3);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(55, 21);
            label12.TabIndex = 65;
            label12.Text = "Seznam";
            // 
            // buttonAddOther
            // 
            buttonAddOther.Location = new Point(369, 445);
            buttonAddOther.Margin = new Padding(4, 5, 4, 5);
            buttonAddOther.Name = "buttonAddOther";
            buttonAddOther.Size = new Size(121, 36);
            buttonAddOther.TabIndex = 64;
            buttonAddOther.Text = "Přidat";
            buttonAddOther.UseVisualStyleBackColor = true;
            buttonAddOther.Click += buttonAddOther_Click;
            // 
            // listBoxOther
            // 
            listBoxOther.BorderStyle = BorderStyle.FixedSingle;
            listBoxOther.FormattingEnabled = true;
            listBoxOther.ItemHeight = 25;
            listBoxOther.Location = new Point(14, 25);
            listBoxOther.Name = "listBoxOther";
            listBoxOther.Size = new Size(476, 327);
            listBoxOther.TabIndex = 61;
            listBoxOther.SelectedIndexChanged += listBoxOther_SelectedIndexChanged;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label13.Location = new Point(14, 379);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new Size(35, 21);
            label13.TabIndex = 63;
            label13.Text = "Typ*";
            // 
            // textBoxOther
            // 
            textBoxOther.Location = new Point(14, 404);
            textBoxOther.Margin = new Padding(5, 8, 5, 8);
            textBoxOther.Name = "textBoxOther";
            textBoxOther.PlaceholderText = "Včely";
            textBoxOther.Size = new Size(476, 28);
            textBoxOther.TabIndex = 62;
            // 
            // ProjectManagementDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(533, 551);
            Controls.Add(tabControl1);
            Controls.Add(label6);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ProjectManagementDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Správa projektů";
            Load += ProjectManagementDialog_Load;
            KeyDown += ProjectManagementDialog_KeyDown;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label6;
        private ListBox listBoxProject;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox textBoxAbsence;
        private Button buttonAddProject;
        private Label label7;
        private Label label9;
        private TextBox textBoxProjectDescription;
        private TextBox textBoxProjectTitle;
        private ComboBox comboBoxProjects;
        private Button buttonAddOperation;
        private ListBox listBoxOperation;
        private Label label10;
        private TextBox textBoxOperation;
        private TabPage tabPage3;
        private Button buttonAddAbsence;
        private ListBox listBoxAbsence;
        private Label label11;
        private TabPage tabPage4;
        private Button buttonAddOther;
        private ListBox listBoxOther;
        private Label label13;
        private TextBox textBoxOther;
        private Label label4;
        private Label label14;
        private Label label8;
        private Label label12;
        private Button buttonArchiveProject;
        private CheckBox checkBoxArchived;
        private GroupBox groupBox2;
        private GroupBox groupBox1;
        private CheckBox checkBoxProposed;
        private Label labelProjectId;
        private Button buttonDeclineAndReplace;
    }
}