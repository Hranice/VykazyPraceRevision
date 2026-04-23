namespace VykazyPrace.Dialogs
{
    partial class UpdateDialog
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
            buttonUpdate = new Button();
            buttonShowChangelog = new Button();
            label1 = new Label();
            labelCurrentVersion = new Label();
            labelLatestVersion = new Label();
            label3 = new Label();
            buttonClose = new Button();
            SuspendLayout();
            // 
            // buttonUpdate
            // 
            buttonUpdate.Location = new Point(12, 12);
            buttonUpdate.Name = "buttonUpdate";
            buttonUpdate.Size = new Size(268, 59);
            buttonUpdate.TabIndex = 0;
            buttonUpdate.Text = "Aktualizovat na nejnovější verzi";
            buttonUpdate.UseVisualStyleBackColor = true;
            buttonUpdate.Click += buttonUpdate_Click;
            // 
            // buttonShowChangelog
            // 
            buttonShowChangelog.Location = new Point(12, 77);
            buttonShowChangelog.Name = "buttonShowChangelog";
            buttonShowChangelog.Size = new Size(304, 37);
            buttonShowChangelog.TabIndex = 1;
            buttonShowChangelog.Text = "Zobrazit seznam změn";
            buttonShowChangelog.UseVisualStyleBackColor = true;
            buttonShowChangelog.Click += buttonShowChangelog_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(286, 12);
            label1.Name = "label1";
            label1.Size = new Size(119, 25);
            label1.TabIndex = 2;
            label1.Text = "Aktuální verze:";
            // 
            // labelCurrentVersion
            // 
            labelCurrentVersion.AutoSize = true;
            labelCurrentVersion.Location = new Point(411, 12);
            labelCurrentVersion.Name = "labelCurrentVersion";
            labelCurrentVersion.Size = new Size(22, 25);
            labelCurrentVersion.TabIndex = 3;
            labelCurrentVersion.Text = "0";
            // 
            // labelLatestVersion
            // 
            labelLatestVersion.AutoSize = true;
            labelLatestVersion.Location = new Point(411, 46);
            labelLatestVersion.Name = "labelLatestVersion";
            labelLatestVersion.Size = new Size(22, 25);
            labelLatestVersion.TabIndex = 5;
            labelLatestVersion.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(286, 46);
            label3.Name = "label3";
            label3.Size = new Size(120, 25);
            label3.TabIndex = 4;
            label3.Text = "Poslední verze:";
            // 
            // buttonClose
            // 
            buttonClose.DialogResult = DialogResult.OK;
            buttonClose.Location = new Point(322, 77);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(156, 37);
            buttonClose.TabIndex = 6;
            buttonClose.Text = "Zavřít";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // UpdateDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(490, 123);
            Controls.Add(buttonClose);
            Controls.Add(labelLatestVersion);
            Controls.Add(label3);
            Controls.Add(labelCurrentVersion);
            Controls.Add(label1);
            Controls.Add(buttonShowChangelog);
            Controls.Add(buttonUpdate);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "UpdateDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Aktualizace";
            Load += UpdateDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonUpdate;
        private Button buttonShowChangelog;
        private Label label1;
        private Label labelCurrentVersion;
        private Label labelLatestVersion;
        private Label label3;
        private Button buttonClose;
    }
}