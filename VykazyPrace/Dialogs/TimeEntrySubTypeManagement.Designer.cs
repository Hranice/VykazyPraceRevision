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
            listBoxTimeEntrySubTypes = new ListBox();
            buttonRemove = new Button();
            SuspendLayout();
            // 
            // listBoxTimeEntrySubTypes
            // 
            listBoxTimeEntrySubTypes.FormattingEnabled = true;
            listBoxTimeEntrySubTypes.ItemHeight = 25;
            listBoxTimeEntrySubTypes.Location = new Point(13, 14);
            listBoxTimeEntrySubTypes.Margin = new Padding(4, 5, 4, 5);
            listBoxTimeEntrySubTypes.Name = "listBoxTimeEntrySubTypes";
            listBoxTimeEntrySubTypes.Size = new Size(262, 279);
            listBoxTimeEntrySubTypes.TabIndex = 0;
            // 
            // buttonRemove
            // 
            buttonRemove.Location = new Point(179, 303);
            buttonRemove.Margin = new Padding(4, 5, 4, 5);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(96, 42);
            buttonRemove.TabIndex = 1;
            buttonRemove.Text = "Smazat";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;
            // 
            // TimeEntrySubTypeManagement
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(288, 359);
            Controls.Add(buttonRemove);
            Controls.Add(listBoxTimeEntrySubTypes);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntrySubTypeManagement";
            Text = "Správa indexů";
            Load += TimeEntrySubTypeManagement_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxTimeEntrySubTypes;
        private Button buttonRemove;
    }
}