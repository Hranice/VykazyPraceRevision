namespace VykazyPrace.Dialogs
{
    partial class ManagerDialog
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
            buttonDownloadArrivalsDepartures = new Button();
            label1 = new Label();
            dateTimePicker1 = new DateTimePicker();
            SuspendLayout();
            // 
            // buttonDownloadArrivalsDepartures
            // 
            buttonDownloadArrivalsDepartures.Location = new Point(218, 30);
            buttonDownloadArrivalsDepartures.Name = "buttonDownloadArrivalsDepartures";
            buttonDownloadArrivalsDepartures.Size = new Size(276, 23);
            buttonDownloadArrivalsDepartures.TabIndex = 0;
            buttonDownloadArrivalsDepartures.Text = "Stáhnout příchody a odchody z PowerKey";
            buttonDownloadArrivalsDepartures.UseVisualStyleBackColor = true;
            buttonDownloadArrivalsDepartures.Click += buttonDownloadArrivalsDepartures_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 2;
            label1.Text = "Měsíc";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(12, 30);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(200, 23);
            dateTimePicker1.TabIndex = 3;
            // 
            // ManagerDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(dateTimePicker1);
            Controls.Add(label1);
            Controls.Add(buttonDownloadArrivalsDepartures);
            Name = "ManagerDialog";
            Text = "ManagerDialog";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonDownloadArrivalsDepartures;
        private Label label1;
        private DateTimePicker dateTimePicker1;
    }
}