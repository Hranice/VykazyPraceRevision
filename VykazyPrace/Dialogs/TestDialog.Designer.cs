namespace VykazyPrace.Dialogs
{
    partial class TestDialog
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
            calendarV21 = new UserControls.CalendarV2.CalendarV2();
            SuspendLayout();
            // 
            // calendarV21
            // 
            calendarV21.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            calendarV21.Location = new Point(12, 12);
            calendarV21.Name = "calendarV21";
            calendarV21.Size = new Size(1067, 537);
            calendarV21.TabIndex = 0;
            // 
            // TestDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1584, 861);
            Controls.Add(calendarV21);
            Name = "TestDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TestDialog";
            ResumeLayout(false);
        }

        #endregion

        private UserControls.CalendarV2.CalendarV2 calendarV21;
    }
}