namespace VykazyPrace.Dialogs
{
    partial class AboutDialog
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
            label2 = new Label();
            pictureBox1 = new PictureBox();
            label1 = new Label();
            label3 = new Label();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Reddit Sans", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label2.Location = new Point(12, 79);
            label2.Name = "label2";
            label2.Size = new Size(77, 25);
            label2.TabIndex = 1;
            label2.Text = "WorkLog";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.worklog;
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(77, 64);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label1.Location = new Point(118, 17);
            label1.Name = "label1";
            label1.Size = new Size(191, 25);
            label1.TabIndex = 3;
            label1.Text = "Program na výkazy hodin";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label3.Location = new Point(118, 42);
            label3.Name = "label3";
            label3.Size = new Size(85, 25);
            label3.TabIndex = 4;
            label3.Text = "Verze 1.0.1";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Reddit Sans Light", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label4.Location = new Point(118, 77);
            label4.Name = "label4";
            label4.Size = new Size(325, 25);
            label4.TabIndex = 5;
            label4.Text = "2025  Jan Procházka, Heinz-Glas Decor, s.r.o";
            // 
            // AboutDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(466, 117);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Controls.Add(label2);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "AboutDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "O programu";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label2;
        private PictureBox pictureBox1;
        private Label label1;
        private Label label3;
        private Label label4;
    }
}