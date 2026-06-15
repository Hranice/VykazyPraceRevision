namespace VykazyPrace.Dialogs
{
    partial class UserSelectionDialog
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
            cLBUserGroups = new CheckedListBox();
            label1 = new Label();
            cLBUsers = new CheckedListBox();
            tBSearch = new TextBox();
            label2 = new Label();
            bOk = new Button();
            bCancel = new Button();
            SuspendLayout();
            // 
            // cLBUserGroups
            // 
            cLBUserGroups.FormattingEnabled = true;
            cLBUserGroups.Location = new Point(13, 39);
            cLBUserGroups.Margin = new Padding(4, 5, 4, 5);
            cLBUserGroups.Name = "cLBUserGroups";
            cLBUserGroups.Size = new Size(188, 193);
            cLBUserGroups.TabIndex = 0;
            cLBUserGroups.ItemCheck += ClbGroups_ItemCheck;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 9);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(65, 20);
            label1.TabIndex = 1;
            label1.Text = "Skupiny";
            // 
            // cLBUsers
            // 
            cLBUsers.FormattingEnabled = true;
            cLBUsers.Location = new Point(209, 39);
            cLBUsers.Margin = new Padding(4, 5, 4, 5);
            cLBUsers.Name = "cLBUsers";
            cLBUsers.Size = new Size(352, 193);
            cLBUsers.TabIndex = 2;
            cLBUsers.ItemCheck += ClbUsers_ItemCheck;
            // 
            // tBSearch
            // 
            tBSearch.Location = new Point(209, 258);
            tBSearch.Name = "tBSearch";
            tBSearch.PlaceholderText = "Josef Novák";
            tBSearch.Size = new Size(352, 26);
            tBSearch.TabIndex = 3;
            tBSearch.TextChanged += TxtSearch_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(209, 9);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(74, 20);
            label2.TabIndex = 4;
            label2.Text = "Uživatelé";
            // 
            // bOk
            // 
            bOk.Location = new Point(343, 292);
            bOk.Name = "bOk";
            bOk.Size = new Size(106, 33);
            bOk.TabIndex = 6;
            bOk.Text = "OK";
            bOk.UseVisualStyleBackColor = true;
            bOk.Click += BOk_Click;
            // 
            // bCancel
            // 
            bCancel.DialogResult = DialogResult.Cancel;
            bCancel.Location = new Point(455, 292);
            bCancel.Name = "bCancel";
            bCancel.Size = new Size(106, 33);
            bCancel.TabIndex = 7;
            bCancel.Text = "Zrušit";
            bCancel.UseVisualStyleBackColor = true;
            bCancel.Click += bCancel_Click;
            // 
            // UserSelectionDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(570, 334);
            Controls.Add(bCancel);
            Controls.Add(bOk);
            Controls.Add(label2);
            Controls.Add(tBSearch);
            Controls.Add(cLBUsers);
            Controls.Add(label1);
            Controls.Add(cLBUserGroups);
            Font = new Font("Microsoft Sans Serif", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "UserSelectionDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Výběr uživatele";
            Load += UserSelectionDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox cLBUserGroups;
        private Label label1;
        private CheckedListBox cLBUsers;
        private TextBox tBSearch;
        private Label label2;
        private Button bOk;
        private Button bCancel;
    }
}