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
            checkBox1 = new CheckBox();
            buttonPathToDatabase = new Button();
            labelDatabaseFilePath = new Label();
            buttonCancel = new Button();
            buttonSave = new Button();
            SuspendLayout();
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(88, 119);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(261, 29);
            checkBox1.TabIndex = 0;
            checkBox1.Text = "Minimalizace do systémové lišty";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.Visible = false;
            // 
            // buttonPathToDatabase
            // 
            buttonPathToDatabase.Location = new Point(12, 12);
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
            labelDatabaseFilePath.Location = new Point(178, 18);
            labelDatabaseFilePath.Name = "labelDatabaseFilePath";
            labelDatabaseFilePath.Size = new Size(340, 25);
            labelDatabaseFilePath.TabIndex = 3;
            labelDatabaseFilePath.Text = "Z:\\TS\\jprochazka-sw\\WorkLog\\Db\\WorkLog.db";
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(916, 701);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(101, 37);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Zrušit";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(750, 701);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(160, 37);
            buttonSave.TabIndex = 5;
            buttonSave.Text = "Uložit";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // SettingsDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1029, 750);
            Controls.Add(buttonSave);
            Controls.Add(buttonCancel);
            Controls.Add(labelDatabaseFilePath);
            Controls.Add(buttonPathToDatabase);
            Controls.Add(checkBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "SettingsDialog";
            Text = "Nastavení";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkBox1;
        private Button buttonPathToDatabase;
        private Label labelDatabaseFilePath;
        private Button buttonCancel;
        private Button buttonSave;
    }
}