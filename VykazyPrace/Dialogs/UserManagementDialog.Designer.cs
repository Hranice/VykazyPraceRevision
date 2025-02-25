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
            textBox1 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            textBox2 = new TextBox();
            label3 = new Label();
            label4 = new Label();
            textBox4 = new TextBox();
            comboBox1 = new ComboBox();
            button1 = new Button();
            listBoxUsers = new ListBox();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new Point(384, 128);
            textBox1.Margin = new Padding(4, 5, 4, 5);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(127, 28);
            textBox1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(384, 98);
            label1.Name = "label1";
            label1.Size = new Size(60, 25);
            label1.TabIndex = 1;
            label1.Text = "Jméno";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(384, 161);
            label2.Name = "label2";
            label2.Size = new Size(70, 25);
            label2.TabIndex = 3;
            label2.Text = "Příjmení";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(384, 191);
            textBox2.Margin = new Padding(4, 5, 4, 5);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(127, 28);
            textBox2.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(384, 287);
            label3.Name = "label3";
            label3.Size = new Size(126, 25);
            label3.TabIndex = 7;
            label3.Text = "Úroveň přístupu";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(384, 224);
            label4.Name = "label4";
            label4.Size = new Size(71, 25);
            label4.TabIndex = 5;
            label4.Text = "Os. číslo";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(384, 254);
            textBox4.Margin = new Padding(4, 5, 4, 5);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(127, 28);
            textBox4.TabIndex = 4;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(384, 315);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(127, 33);
            comboBox1.TabIndex = 8;
            // 
            // button1
            // 
            button1.Location = new Point(384, 354);
            button1.Name = "button1";
            button1.Size = new Size(127, 38);
            button1.TabIndex = 9;
            button1.Text = "Přidat";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listBoxUsers
            // 
            listBoxUsers.FormattingEnabled = true;
            listBoxUsers.ItemHeight = 25;
            listBoxUsers.Location = new Point(632, 233);
            listBoxUsers.Name = "listBoxUsers";
            listBoxUsers.Size = new Size(256, 79);
            listBoxUsers.TabIndex = 10;
            // 
            // UserManagementDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1029, 750);
            Controls.Add(listBoxUsers);
            Controls.Add(button1);
            Controls.Add(comboBox1);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(textBox4);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "UserManagementDialog";
            Text = "UserManagementDialog";
            Load += UserManagementDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Label label1;
        private Label label2;
        private TextBox textBox2;
        private Label label3;
        private Label label4;
        private TextBox textBox4;
        private ComboBox comboBox1;
        private Button button1;
        private ListBox listBoxUsers;
    }
}