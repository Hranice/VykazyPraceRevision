namespace VykazyPrace.Dialogs
{
    partial class SettingsDialog
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
            checkBoxMinimizeToTray = new CheckBox();
            buttonPathToDatabase = new Button();
            labelDatabaseFilePath = new Label();
            buttonCancel = new Button();
            buttonSave = new Button();
            groupBox1 = new GroupBox();
            textBoxNotificationText = new TextBox();
            label2 = new Label();
            textBoxNotificationTitle = new TextBox();
            label1 = new Label();
            dateTimePicker1 = new DateTimePicker();
            checkBoxEnableNotification = new CheckBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // checkBoxMinimizeToTray
            // 
            checkBoxMinimizeToTray.AutoSize = true;
            checkBoxMinimizeToTray.Location = new Point(18, 36);
            checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            checkBoxMinimizeToTray.Size = new Size(261, 29);
            checkBoxMinimizeToTray.TabIndex = 0;
            checkBoxMinimizeToTray.Text = "Minimalizace do systémové lišty";
            checkBoxMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // buttonPathToDatabase
            // 
            buttonPathToDatabase.Location = new Point(18, 37);
            buttonPathToDatabase.Name = "buttonPathToDatabase";
            buttonPathToDatabase.Size = new Size(160, 37);
            buttonPathToDatabase.TabIndex = 1;
            buttonPathToDatabase.Text = "Cesta k databázi";
            buttonPathToDatabase.UseVisualStyleBackColor = true;
            buttonPathToDatabase.Click += buttonPathToDatabase_Click;
            // 
            // labelDatabaseFilePath
            // 
            labelDatabaseFilePath.AutoSize = true;
            labelDatabaseFilePath.Location = new Point(184, 43);
            labelDatabaseFilePath.Name = "labelDatabaseFilePath";
            labelDatabaseFilePath.Size = new Size(340, 25);
            labelDatabaseFilePath.TabIndex = 3;
            labelDatabaseFilePath.Text = "Z:\\TS\\jprochazka-sw\\WorkLog\\Db\\WorkLog.db";
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(770, 407);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(101, 37);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Zrušit";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            buttonSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSave.Location = new Point(604, 407);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(160, 37);
            buttonSave.TabIndex = 5;
            buttonSave.Text = "Uložit";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(textBoxNotificationText);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(textBoxNotificationTitle);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(dateTimePicker1);
            groupBox1.Controls.Add(checkBoxEnableNotification);
            groupBox1.Location = new Point(12, 118);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(859, 173);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Připomenutí";
            // 
            // textBoxNotificationText
            // 
            textBoxNotificationText.Location = new Point(206, 108);
            textBoxNotificationText.MaxLength = 160;
            textBoxNotificationText.Name = "textBoxNotificationText";
            textBoxNotificationText.Size = new Size(647, 28);
            textBoxNotificationText.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(206, 80);
            label2.Name = "label2";
            label2.Size = new Size(134, 25);
            label2.TabIndex = 5;
            label2.Text = "Text připomenutí";
            // 
            // textBoxNotificationTitle
            // 
            textBoxNotificationTitle.Location = new Point(18, 108);
            textBoxNotificationTitle.MaxLength = 20;
            textBoxNotificationTitle.Name = "textBoxNotificationTitle";
            textBoxNotificationTitle.Size = new Size(182, 28);
            textBoxNotificationTitle.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 80);
            label1.Name = "label1";
            label1.Size = new Size(153, 25);
            label1.TabIndex = 3;
            label1.Text = "Nadpis připomenutí";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Format = DateTimePickerFormat.Time;
            dateTimePicker1.Location = new Point(206, 37);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.ShowUpDown = true;
            dateTimePicker1.Size = new Size(101, 28);
            dateTimePicker1.TabIndex = 2;
            dateTimePicker1.Value = new DateTime(2025, 6, 11, 9, 23, 0, 0);
            // 
            // checkBoxEnableNotification
            // 
            checkBoxEnableNotification.AutoSize = true;
            checkBoxEnableNotification.Location = new Point(18, 36);
            checkBoxEnableNotification.Name = "checkBoxEnableNotification";
            checkBoxEnableNotification.Size = new Size(182, 29);
            checkBoxEnableNotification.TabIndex = 1;
            checkBoxEnableNotification.Text = "Zapnout připomenutí";
            checkBoxEnableNotification.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(buttonPathToDatabase);
            groupBox2.Controls.Add(labelDatabaseFilePath);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(859, 100);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "Databáze";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(checkBoxMinimizeToTray);
            groupBox3.Location = new Point(12, 297);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(859, 88);
            groupBox3.TabIndex = 8;
            groupBox3.TabStop = false;
            groupBox3.Text = "Různé";
            // 
            // SettingsDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(883, 456);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(buttonSave);
            Controls.Add(buttonCancel);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "SettingsDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Nastavení";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CheckBox checkBoxMinimizeToTray;
        private Button buttonPathToDatabase;
        private Label labelDatabaseFilePath;
        private Button buttonCancel;
        private Button buttonSave;
        private GroupBox groupBox1;
        private CheckBox checkBoxEnableNotification;
        private DateTimePicker dateTimePicker1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private TextBox textBoxNotificationText;
        private Label label2;
        private TextBox textBoxNotificationTitle;
        private Label label1;
    }
}