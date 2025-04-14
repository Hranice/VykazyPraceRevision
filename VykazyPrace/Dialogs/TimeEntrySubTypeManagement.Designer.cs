namespace VykazyPrace.Dialogs
{
    partial class TimeEntrySubTypeManagement
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
            panel1 = new Panel();
            buttonSave = new Button();
            buttonRemove = new Button();
            textBoxTitle = new TextBox();
            listBoxTimeEntrySubTypes = new ListBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(buttonSave);
            panel1.Controls.Add(buttonRemove);
            panel1.Controls.Add(textBoxTitle);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 285);
            panel1.Name = "panel1";
            panel1.Size = new Size(288, 74);
            panel1.TabIndex = 4;
            // 
            // buttonSave
            // 
            buttonSave.Dock = DockStyle.Fill;
            buttonSave.Location = new Point(0, 28);
            buttonSave.Margin = new Padding(4, 5, 4, 5);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(188, 46);
            buttonSave.TabIndex = 8;
            buttonSave.Text = "Uložit";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // buttonRemove
            // 
            buttonRemove.Dock = DockStyle.Right;
            buttonRemove.Location = new Point(188, 28);
            buttonRemove.Margin = new Padding(4, 5, 4, 5);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(100, 46);
            buttonRemove.TabIndex = 7;
            buttonRemove.Text = "Smazat";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;
            // 
            // textBoxTitle
            // 
            textBoxTitle.Dock = DockStyle.Top;
            textBoxTitle.Location = new Point(0, 0);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new Size(288, 28);
            textBoxTitle.TabIndex = 4;
            // 
            // listBoxTimeEntrySubTypes
            // 
            listBoxTimeEntrySubTypes.Dock = DockStyle.Fill;
            listBoxTimeEntrySubTypes.FormattingEnabled = true;
            listBoxTimeEntrySubTypes.ItemHeight = 25;
            listBoxTimeEntrySubTypes.Location = new Point(0, 0);
            listBoxTimeEntrySubTypes.Margin = new Padding(4, 5, 4, 5);
            listBoxTimeEntrySubTypes.Name = "listBoxTimeEntrySubTypes";
            listBoxTimeEntrySubTypes.Size = new Size(288, 285);
            listBoxTimeEntrySubTypes.TabIndex = 5;
            listBoxTimeEntrySubTypes.SelectedIndexChanged += listBoxTimeEntrySubTypes_SelectedIndexChanged;
            // 
            // TimeEntrySubTypeManagement
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(288, 359);
            Controls.Add(listBoxTimeEntrySubTypes);
            Controls.Add(panel1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntrySubTypeManagement";
            Text = "Správa indexů";
            Load += TimeEntrySubTypeManagement_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button buttonSave;
        private Button buttonRemove;
        private TextBox textBoxTitle;
        private ListBox listBoxTimeEntrySubTypes;
    }
}