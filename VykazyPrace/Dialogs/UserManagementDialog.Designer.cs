namespace VykazyPrace.Dialogs
{
    partial class UserManagementDialog
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
            button2 = new Button();
            label5 = new Label();
            textBox3 = new TextBox();
            label6 = new Label();
            listBox1 = new ListBox();
            groupBox1 = new GroupBox();
            button5 = new Button();
            label8 = new Label();
            textBox6 = new TextBox();
            label7 = new Label();
            textBox5 = new TextBox();
            label9 = new Label();
            textBox7 = new TextBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // button2
            // 
            button2.Location = new Point(12, 163);
            button2.Margin = new Padding(4, 5, 4, 5);
            button2.Name = "button2";
            button2.Size = new Size(147, 36);
            button2.TabIndex = 19;
            button2.Text = "Přidat";
            button2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label5.Location = new Point(12, 36);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(53, 21);
            label5.TabIndex = 14;
            label5.Text = "Jméno*";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(12, 61);
            textBox3.Margin = new Padding(5, 8, 5, 8);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(121, 28);
            textBox3.TabIndex = 13;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(12, 9);
            label6.Name = "label6";
            label6.Size = new Size(96, 17);
            label6.TabIndex = 28;
            label6.Text = "Seznam uživatelů:";
            // 
            // listBox1
            // 
            listBox1.BorderStyle = BorderStyle.FixedSingle;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 25;
            listBox1.Items.AddRange(new object[] { "Jan Procházka" });
            listBox1.Location = new Point(14, 29);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(275, 77);
            listBox1.TabIndex = 27;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(textBox5);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(textBox7);
            groupBox1.Controls.Add(button5);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(textBox6);
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Location = new Point(14, 112);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(277, 206);
            groupBox1.TabIndex = 26;
            groupBox1.TabStop = false;
            groupBox1.Text = "Přidání uživatele";
            // 
            // button5
            // 
            button5.Location = new Point(167, 162);
            button5.Margin = new Padding(4, 5, 4, 5);
            button5.Name = "button5";
            button5.Size = new Size(97, 36);
            button5.TabIndex = 20;
            button5.Text = "Odstranit";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label8.Location = new Point(143, 36);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(61, 21);
            label8.TabIndex = 16;
            label8.Text = "Příjmení*";
            // 
            // textBox6
            // 
            textBox6.Location = new Point(143, 60);
            textBox6.Margin = new Padding(5, 8, 5, 8);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(121, 28);
            textBox6.TabIndex = 15;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label7.Location = new Point(143, 97);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(126, 21);
            label7.TabIndex = 24;
            label7.Text = "Windows už. jméno*";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(143, 121);
            textBox5.Margin = new Padding(5, 8, 5, 8);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(121, 28);
            textBox5.TabIndex = 23;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label9.Location = new Point(12, 97);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(62, 21);
            label9.TabIndex = 22;
            label9.Text = "Os. číslo*";
            // 
            // textBox7
            // 
            textBox7.Location = new Point(12, 122);
            textBox7.Margin = new Padding(5, 8, 5, 8);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(121, 28);
            textBox7.TabIndex = 21;
            // 
            // UserManagementDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(304, 329);
            Controls.Add(label6);
            Controls.Add(listBox1);
            Controls.Add(groupBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "UserManagementDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "UserManagementDialog";
            Load += UserManagementDialog_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button2;
        private Label label5;
        private TextBox textBox3;
        private Label label6;
        private ListBox listBox1;
        private GroupBox groupBox1;
        private Button button5;
        private Label label8;
        private TextBox textBox6;
        private Label label7;
        private TextBox textBox5;
        private Label label9;
        private TextBox textBox7;
    }
}