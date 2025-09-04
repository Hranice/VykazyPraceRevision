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
            numericUpDown1 = new NumericUpDown();
            label2 = new Label();
            checkBox1 = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // buttonDownloadArrivalsDepartures
            // 
            buttonDownloadArrivalsDepartures.Location = new Point(344, 30);
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
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(218, 30);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(120, 23);
            numericUpDown1.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(218, 9);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 5;
            label2.Text = "Os. číslo";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(218, 59);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(113, 19);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Všichni uživatelé";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // ManagerDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(checkBox1);
            Controls.Add(label2);
            Controls.Add(numericUpDown1);
            Controls.Add(dateTimePicker1);
            Controls.Add(label1);
            Controls.Add(buttonDownloadArrivalsDepartures);
            Name = "ManagerDialog";
            Text = "ManagerDialog";
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonDownloadArrivalsDepartures;
        private Label label1;
        private DateTimePicker dateTimePicker1;
        private Label label2;
        public NumericUpDown numericUpDown1;
        private CheckBox checkBox1;
    }
}