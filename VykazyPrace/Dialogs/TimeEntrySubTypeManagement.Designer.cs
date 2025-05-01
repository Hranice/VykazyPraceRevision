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
            tableLayoutPanel1 = new TableLayoutPanel();
            listBoxTimeEntrySubTypes = new ListBox();
            panel1 = new Panel();
            panel2 = new Panel();
            buttonRename = new Button();
            buttonDelete = new Button();
            textBoxTitle = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(listBoxTimeEntrySubTypes, 0, 0);
            tableLayoutPanel1.Controls.Add(panel1, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(5, 5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tableLayoutPanel1.Size = new Size(525, 349);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // listBoxTimeEntrySubTypes
            // 
            listBoxTimeEntrySubTypes.Dock = DockStyle.Fill;
            listBoxTimeEntrySubTypes.FormattingEnabled = true;
            listBoxTimeEntrySubTypes.ItemHeight = 25;
            listBoxTimeEntrySubTypes.Location = new Point(4, 5);
            listBoxTimeEntrySubTypes.Margin = new Padding(4, 5, 4, 5);
            listBoxTimeEntrySubTypes.Name = "listBoxTimeEntrySubTypes";
            listBoxTimeEntrySubTypes.SelectionMode = SelectionMode.MultiExtended;
            listBoxTimeEntrySubTypes.Size = new Size(254, 339);
            listBoxTimeEntrySubTypes.TabIndex = 7;
            listBoxTimeEntrySubTypes.SelectedIndexChanged += listBoxTimeEntrySubTypes_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(textBoxTitle);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(265, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(257, 64);
            panel1.TabIndex = 8;
            // 
            // panel2
            // 
            panel2.Controls.Add(buttonRename);
            panel2.Controls.Add(buttonDelete);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 28);
            panel2.Name = "panel2";
            panel2.Size = new Size(257, 36);
            panel2.TabIndex = 14;
            // 
            // buttonRename
            // 
            buttonRename.Dock = DockStyle.Fill;
            buttonRename.Location = new Point(100, 0);
            buttonRename.Margin = new Padding(4, 5, 4, 5);
            buttonRename.Name = "buttonRename";
            buttonRename.Size = new Size(157, 36);
            buttonRename.TabIndex = 11;
            buttonRename.Text = "Přejmenovat";
            buttonRename.UseVisualStyleBackColor = true;
            buttonRename.Click += buttonRename_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Dock = DockStyle.Left;
            buttonDelete.Location = new Point(0, 0);
            buttonDelete.Margin = new Padding(4, 5, 4, 5);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(100, 36);
            buttonDelete.TabIndex = 10;
            buttonDelete.Text = "Smazat";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += buttonDelete_Click;
            // 
            // textBoxTitle
            // 
            textBoxTitle.Dock = DockStyle.Top;
            textBoxTitle.Location = new Point(0, 0);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new Size(257, 28);
            textBoxTitle.TabIndex = 13;
            // 
            // TimeEntrySubTypeManagement
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(535, 359);
            Controls.Add(tableLayoutPanel1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntrySubTypeManagement";
            Padding = new Padding(5);
            Text = "Správa indexů";
            Load += TimeEntrySubTypeManagement_Load;
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private ListBox listBoxTimeEntrySubTypes;
        private Panel panel1;
        private Panel panel2;
        private Button buttonRename;
        private Button buttonDelete;
        private TextBox textBoxTitle;
    }
}