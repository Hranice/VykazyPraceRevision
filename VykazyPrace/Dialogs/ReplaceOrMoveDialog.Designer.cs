namespace VykazyPrace.Dialogs
{
    partial class ReplaceOrMoveDialog
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
            label1 = new Label();
            pictureBox1 = new PictureBox();
            panel1 = new Panel();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(67, 25);
            label1.Name = "label1";
            label1.Size = new Size(266, 30);
            label1.TabIndex = 0;
            label1.Text = "Na této pozici již existuje záznam.\r\nChcete ho nahradit, nebo posunout vše doprava?";
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(29, 23);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(32, 32);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ButtonFace;
            panel1.Controls.Add(button3);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 71);
            panel1.Name = "panel1";
            panel1.Size = new Size(387, 42);
            panel1.TabIndex = 5;
            // 
            // button3
            // 
            button3.DialogResult = DialogResult.Cancel;
            button3.Location = new Point(310, 10);
            button3.Name = "button3";
            button3.Size = new Size(65, 23);
            button3.TabIndex = 6;
            button3.Text = "Zrušit";
            button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.No;
            button2.Location = new Point(188, 10);
            button2.Name = "button2";
            button2.Size = new Size(116, 23);
            button2.TabIndex = 5;
            button2.Text = "Posunout ostatní";
            button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.Yes;
            button1.Location = new Point(105, 10);
            button1.Name = "button1";
            button1.Size = new Size(77, 23);
            button1.TabIndex = 4;
            button1.Text = "Nahradit";
            button1.UseVisualStyleBackColor = true;
            // 
            // ReplaceOrMoveDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(387, 113);
            Controls.Add(pictureBox1);
            Controls.Add(label1);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "ReplaceOrMoveDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Kolize záznamu";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private PictureBox pictureBox1;
        private Panel panel1;
        private Button button3;
        private Button button2;
        private Button button1;
    }
}