namespace VykazyPrace.Dialogs
{
    partial class ProjectManagementDialog
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
            button1 = new Button();
            label2 = new Label();
            textBox2 = new TextBox();
            panel1 = new Panel();
            button2 = new Button();
            button3 = new Button();
            groupBox1 = new GroupBox();
            label6 = new Label();
            listBox1 = new ListBox();
            label1 = new Label();
            textBox1 = new TextBox();
            textBox3 = new TextBox();
            label5 = new Label();
            button4 = new Button();
            panel1.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 219);
            button1.Margin = new Padding(4, 5, 4, 5);
            button1.Name = "button1";
            button1.Size = new Size(268, 36);
            button1.TabIndex = 19;
            button1.Text = "Přidat";
            button1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label2.Location = new Point(12, 36);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(121, 21);
            label2.TabIndex = 14;
            label2.Text = "Označení projektu*";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 61);
            textBox2.Margin = new Padding(5, 8, 5, 8);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(121, 28);
            textBox2.TabIndex = 13;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button3);
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(400, 33);
            panel1.TabIndex = 21;
            // 
            // button2
            // 
            button2.BackColor = Color.White;
            button2.Dock = DockStyle.Left;
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Reddit Sans", 10F, FontStyle.Bold);
            button2.Location = new Point(0, 0);
            button2.Name = "button2";
            button2.Size = new Size(200, 31);
            button2.TabIndex = 3;
            button2.Text = "PROJEKT";
            button2.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            button3.BackColor = SystemColors.AppWorkspace;
            button3.Dock = DockStyle.Right;
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("Reddit Sans", 10F);
            button3.Location = new Point(198, 0);
            button3.Name = "button3";
            button3.Size = new Size(200, 31);
            button3.TabIndex = 4;
            button3.Text = "ZAKÁZKA";
            button3.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button4);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Location = new Point(12, 167);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(400, 265);
            groupBox1.TabIndex = 22;
            groupBox1.TabStop = false;
            groupBox1.Text = "Přidání projektu";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(10, 55);
            label6.Name = "label6";
            label6.Size = new Size(93, 17);
            label6.TabIndex = 24;
            label6.Text = "Seznam projektů:";
            // 
            // listBox1
            // 
            listBox1.BorderStyle = BorderStyle.FixedSingle;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 25;
            listBox1.Items.AddRange(new object[] { "0230I24 - Automatické balení po pecích a UV", "tes", "test", "test", "test", "tes" });
            listBox1.Location = new Point(12, 75);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(399, 77);
            listBox1.TabIndex = 23;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label1.Location = new Point(143, 36);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(104, 21);
            label1.TabIndex = 16;
            label1.Text = "Název projektu*";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(143, 61);
            textBox1.Margin = new Padding(5, 8, 5, 8);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(240, 28);
            textBox1.TabIndex = 15;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(12, 122);
            textBox3.Margin = new Padding(5, 8, 5, 8);
            textBox3.Multiline = true;
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(371, 84);
            textBox3.TabIndex = 17;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label5.Location = new Point(12, 97);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(69, 21);
            label5.TabIndex = 18;
            label5.Text = "Poznámka";
            // 
            // button4
            // 
            button4.Location = new Point(288, 219);
            button4.Margin = new Padding(4, 5, 4, 5);
            button4.Name = "button4";
            button4.Size = new Size(95, 36);
            button4.TabIndex = 20;
            button4.Text = "Zavřít";
            button4.UseVisualStyleBackColor = true;
            // 
            // ProjectManagementDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(424, 442);
            Controls.Add(label6);
            Controls.Add(listBox1);
            Controls.Add(panel1);
            Controls.Add(groupBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ProjectManagementDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Správa projektů";
            panel1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button1;
        private Label label2;
        private TextBox textBox2;
        private Panel panel1;
        private Button button2;
        private Button button3;
        private GroupBox groupBox1;
        private Label label6;
        private ListBox listBox1;
        private Label label1;
        private TextBox textBox1;
        private Label label5;
        private TextBox textBox3;
        private Button button4;
    }
}