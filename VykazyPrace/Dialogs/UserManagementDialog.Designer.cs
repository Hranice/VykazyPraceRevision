namespace VykazyPrace.Dialogs
{
    partial class UserManagementDialog
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
            buttonAdd = new Button();
            label5 = new Label();
            textBoxFirstName = new TextBox();
            label6 = new Label();
            listBoxUsers = new ListBox();
            groupBox1 = new GroupBox();
            buttonGenerateWindowsUsername = new Button();
            label1 = new Label();
            numericUpDownLevelOfAccess = new NumericUpDown();
            maskedTextBoxPersonalNumber = new MaskedTextBox();
            label7 = new Label();
            textBoxWindowsUsername = new TextBox();
            label9 = new Label();
            buttonRemove = new Button();
            label8 = new Label();
            textBoxSurname = new TextBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownLevelOfAccess).BeginInit();
            SuspendLayout();
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(12, 163);
            buttonAdd.Margin = new Padding(4, 5, 4, 5);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(147, 36);
            buttonAdd.TabIndex = 19;
            buttonAdd.Text = "Přidat";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label5.Location = new Point(12, 36);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(53, 21);
            label5.TabIndex = 14;
            label5.Text = "Jméno*";
            // 
            // textBoxFirstName
            // 
            textBoxFirstName.Location = new Point(12, 61);
            textBoxFirstName.Margin = new Padding(5, 8, 5, 8);
            textBoxFirstName.Name = "textBoxFirstName";
            textBoxFirstName.Size = new Size(121, 28);
            textBoxFirstName.TabIndex = 13;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(12, 9);
            label6.Name = "label6";
            label6.Size = new Size(96, 17);
            label6.TabIndex = 28;
            label6.Text = "Seznam uživatelů:";
            // 
            // listBoxUsers
            // 
            listBoxUsers.BorderStyle = BorderStyle.FixedSingle;
            listBoxUsers.FormattingEnabled = true;
            listBoxUsers.ItemHeight = 25;
            listBoxUsers.Location = new Point(14, 29);
            listBoxUsers.Name = "listBoxUsers";
            listBoxUsers.Size = new Size(275, 77);
            listBoxUsers.TabIndex = 27;
            listBoxUsers.SelectedIndexChanged += listBoxUsers_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(buttonGenerateWindowsUsername);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(numericUpDownLevelOfAccess);
            groupBox1.Controls.Add(maskedTextBoxPersonalNumber);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(textBoxWindowsUsername);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(buttonRemove);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(textBoxSurname);
            groupBox1.Controls.Add(buttonAdd);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(textBoxFirstName);
            groupBox1.Location = new Point(14, 112);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(277, 206);
            groupBox1.TabIndex = 26;
            groupBox1.TabStop = false;
            groupBox1.Text = "Přidání uživatele";
            // 
            // buttonGenerateWindowsUsername
            // 
            buttonGenerateWindowsUsername.FlatAppearance.BorderColor = Color.FromArgb(122, 122, 122);
            buttonGenerateWindowsUsername.FlatStyle = FlatStyle.Flat;
            buttonGenerateWindowsUsername.Font = new Font("Roboto", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            buttonGenerateWindowsUsername.Location = new Point(175, 121);
            buttonGenerateWindowsUsername.Margin = new Padding(0);
            buttonGenerateWindowsUsername.Name = "buttonGenerateWindowsUsername";
            buttonGenerateWindowsUsername.Size = new Size(28, 28);
            buttonGenerateWindowsUsername.TabIndex = 28;
            buttonGenerateWindowsUsername.Text = "↻";
            buttonGenerateWindowsUsername.UseVisualStyleBackColor = true;
            buttonGenerateWindowsUsername.Click += buttonGenerateWindowsUsername_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label1.Location = new Point(218, 96);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(55, 21);
            label1.TabIndex = 27;
            label1.Text = "Přístup*";
            // 
            // numericUpDownLevelOfAccess
            // 
            numericUpDownLevelOfAccess.Location = new Point(218, 121);
            numericUpDownLevelOfAccess.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            numericUpDownLevelOfAccess.Name = "numericUpDownLevelOfAccess";
            numericUpDownLevelOfAccess.Size = new Size(40, 28);
            numericUpDownLevelOfAccess.TabIndex = 26;
            numericUpDownLevelOfAccess.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // maskedTextBoxPersonalNumber
            // 
            maskedTextBoxPersonalNumber.Location = new Point(12, 121);
            maskedTextBoxPersonalNumber.Mask = "0000";
            maskedTextBoxPersonalNumber.Name = "maskedTextBoxPersonalNumber";
            maskedTextBoxPersonalNumber.Size = new Size(53, 28);
            maskedTextBoxPersonalNumber.TabIndex = 25;
            maskedTextBoxPersonalNumber.ValidatingType = typeof(int);
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label7.Location = new Point(82, 97);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(100, 21);
            label7.TabIndex = 24;
            label7.Text = "Windows login*";
            // 
            // textBoxWindowsUsername
            // 
            textBoxWindowsUsername.Location = new Point(82, 121);
            textBoxWindowsUsername.Margin = new Padding(5, 8, 5, 8);
            textBoxWindowsUsername.Name = "textBoxWindowsUsername";
            textBoxWindowsUsername.Size = new Size(94, 28);
            textBoxWindowsUsername.TabIndex = 23;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label9.Location = new Point(12, 97);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(62, 21);
            label9.TabIndex = 22;
            label9.Text = "Os. číslo*";
            // 
            // buttonRemove
            // 
            buttonRemove.Location = new Point(167, 162);
            buttonRemove.Margin = new Padding(4, 5, 4, 5);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(97, 36);
            buttonRemove.TabIndex = 20;
            buttonRemove.Text = "Odstranit";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label8.Location = new Point(143, 36);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(61, 21);
            label8.TabIndex = 16;
            label8.Text = "Příjmení*";
            // 
            // textBoxSurname
            // 
            textBoxSurname.Location = new Point(143, 60);
            textBoxSurname.Margin = new Padding(5, 8, 5, 8);
            textBoxSurname.Name = "textBoxSurname";
            textBoxSurname.Size = new Size(121, 28);
            textBoxSurname.TabIndex = 15;
            // 
            // UserManagementDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(304, 323);
            Controls.Add(label6);
            Controls.Add(listBoxUsers);
            Controls.Add(groupBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "UserManagementDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "UserManagementDialog";
            Load += UserManagementDialog_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownLevelOfAccess).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonAdd;
        private Label label5;
        private TextBox textBoxFirstName;
        private Label label6;
        private ListBox listBoxUsers;
        private GroupBox groupBox1;
        private Button buttonRemove;
        private Label label8;
        private TextBox textBoxSurname;
        private Label label7;
        private TextBox textBoxWindowsUsername;
        private Label label9;
        private MaskedTextBox maskedTextBoxPersonalNumber;
        private Label label1;
        private NumericUpDown numericUpDownLevelOfAccess;
        private Button buttonGenerateWindowsUsername;
    }
}